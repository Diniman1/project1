const express = require('express');
const config = require('./config/default.json');
const argon = require('argon2');
const mysql = require('mysql2');
const Session = require('./models/Session');
const exSession = require('express-session');
const uuid = require('uuid4');
const {json} = require("express");

const db = mysql.createConnection(config.mysqlConfig);
const app = express();

app.use(express.json());
app.use(exSession({
    secret: config.secretKey,
    saveUninitialized: true,
    cookie: { maxAge: 1000*60*60 }
}));

async function start() {
    try {
        db.connect((err) => {
            if(err) console.log(err);
            else console.log('Mysql connected...');
        });
        app.listen(config.port, config.host, () =>
            console.log(`Server listening on http://${config.host}:${config.port}`));
    } catch (e) {
        console.log(`Server error ${e}`);
        process.exit(1);
    }
}
start();

/*async function setHash(){
    const hash = await argon.hash('1234');
    console.log(hash);
}
setHash();*/

app.get('/', (req, res) => {
    if(!req.session.sid) {
        req.session.sid = uuid();
        Session.add(req.session);
    }
    res.status(200).json();
});
app.get('/logout', (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.sendStatus(401);
    }
    Session.logout(req.session.sid);
    req.session.destroy();
    res.status(200).json();
});

app.get('/getAllStatus', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    getAllStatus().then((result) => {
        res.status(200).json({ result });
    }).catch(e => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибка сервера. Попробуйте снова.'});
    });
});
app.get('/getAllCategory', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    getAllCategory().then((result) => {
        res.status(200).json({ result });
    }).catch(e => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибка сервера. Попробуйте снова.'});
    });
});
app.get('/getAllWorker', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 1) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }
    getAllWorkers().then((result) => {
        res.status(200).json({ result });
    }).catch(e => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Список механиков не был загружен.'});
    });
});
app.get('/getAllSpareParts', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }

    getAllSpareParts().then((result) => {
        res.status(200).json({ result });
    }).catch(e => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Список запчастей не был загружен.'});
    });
});

app.get('/getApplicationForMechanic', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    getApplicationForMechanic(Session.getUser(req.session.sid)['Id']).then(result => {
        res.status(200).json({ result });
    }).catch(e => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Текущие ремонты не были загружены. Попробуйте снова.'});
    });
});


app.post('/auth', async (req, res) => {
    if(!req.session.sid) {
        req.session.sid = uuid();
        Session.add(req.session);
    }
    if(Session.isAuth(req.session.sid)) {
        return res.status(208).json({
            roles: Session.getUser(req.session.sid)['Id_posts'],
            name: Session.getUser(req.session.sid)['Full_name']
        });
    }
    let {login, password} = req.body;
    const user = await getUserByLogin(login);
    if(!user) {
        return res.status(401).json({message: 'Пользователь не найден. Поробуйте снова.'});
    }
    const hasPassword = await argon.verify(user['Password'], password);
    if(!hasPassword){
        return res.status(401).json({message: 'Неверный пароль. Попробуйте снова.'});
    }
    Session.authorize(req.session.sid, user);
    res.status(200).json({
        roles: user['Id_posts'],
        name: user['Full_name']
    });
});
app.post('/setStatus', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }

    const { status, id } = req.body;
    if(status === "" || id < 1) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }
    const application = await getApplicationById(id);
    if(!application) return res.status(404).json({message: 'Такой заявки не существует!'});

    const allStatus = await getAllStatus();
    const statusId = allStatus.findIndex(e => e["Name"] === status);
    setStatus(statusId+1, id).then((result) => {
        res.sendStatus(200);
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Статус не был установлен.'});
    });
});
app.post('/setDateEnd', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 1) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }

    const { id } = req.body;
    const application = await getApplicationById(id);
    if(!application) return res.status(404).json({message: 'Такой заявки не существует!'});

    setDateEnd(id).then((result) => {
        res.sendStatus(200);
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Статус не был установлен.'});
    });
});

app.post('/getClient', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    const { telephone } = req.body;
    if(!/^(\d){11}$/.test(telephone)) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }
    const client = await getClientByTelephone(telephone);
    if(!client) {
        return res.status(404).json({message: 'Нет клиента с таким номером телефона'});
    }
    res.status(200).json({ client });
});
app.post('/getCar', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    const { number } = req.body;
    if(!/[АВСЕНКМОРТХУ]\d{3}[АВСЕНКМОРТХУ]{2}\d{2,3}$/g.test(number)) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }
    const car = await getCarByNumber(number);
    if(!car) {
        return res.status(404).json({message: 'Нет автомобиля с таким гос. номером.'});
    }
    res.status(200).json({ car });
});
app.post('/getApplication', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    const { typeSort, filter } = req.body;
    if(typeSort === "All"){
        getAllApplication().then(result => {
            res.status(200).json({ result });
        }).catch(e => {
            console.log(`[MySQL] ${e}`);
            res.status(500).json({message: 'Заявки по фильтру (Все) не были загружены. Попробуйте снова.'});
        });
    } else if(typeSort === "Month"){
        getApplicationByMonth().then(result => {
            res.status(200).json({ result });
        }).catch(e => {
            console.log(`[MySQL] ${e}`);
            res.status(500).json({message: 'Заявки по фильтру (Текущий месяц) не были загружены. Попробуйте снова.'});
        });
    } else if(typeSort === "Date"){
        getApplicationByDate(filter).then(result => {
            res.status(200).json({ result });
        }).catch(e => {
            console.log(`[MySQL] ${e}`);
            res.status(500).json({message: 'Заявки по фильтру (Дата) не были загружены. Попробуйте снова.'});
        });
    } else if(typeSort === "Client"){
        getApplicationByClient(filter).then(result => {
            res.status(200).json({ result });
        }).catch(e => {
            console.log(`[MySQL] ${e}`);
            res.status(500).json({message: 'Заявки по фильтру (Клиент) не были загружены. Попробуйте снова.'});
        });
    } else if(typeSort === "Car"){
        getApplicationByCar(filter).then(result => {
            res.status(200).json({ result });
        }).catch(e => {
            console.log(`[MySQL] ${e}`);
            res.status(500).json({message: 'Заявки по фильтру (Автомобиль) не были загружены. Попробуйте снова.'});
        });
    } else if(typeSort === "Status"){
        getApplicationByStatus(filter).then(result => {
            res.status(200).json({ result });
        }).catch(e => {
            console.log(`[MySQL] ${e}`);
            res.status(500).json({message: 'Заявки по фильтру (Статус) не были загружены. Попробуйте снова.'});
        });
    } else {
        res.status(404).json({ message: 'Заявки по фильтру (Неизвестно) не были загружены. Попробуйте снова.' });
    }
});
app.post('/getRepair', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }

    const { id } = req.body;
    if(id < 1) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }
    const application = await getApplicationById(id);
    if(!application) return res.status(404).json({message: 'Такой заявки не существует!'});

    getRepairById(id).then((result) => {
        res.status(200).json({ result });
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибка сервера при получении данных.'});
    });
});
app.post('/getRepairServices', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }

    const { repairId } = req.body;
    if(repairId < 1) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }

    getRepairServicesById(repairId).then((result) => {
        res.status(200).json({result: result});
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибки сервера при поиске данных.'});
    });
});
app.post('/getGivenPart', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }

    const { repairId } = req.body;
    if(repairId < 1) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }

    getGivenPartById(repairId).then((result) => {
        res.status(200).json({result: result});
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибки сервера при поиске данных.'});
    });
});
app.post('/getServiceByCategory', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    const { categoryId } = req.body;
    getServiceByCategory(categoryId).then((result) => {
        res.status(200).json({ result });
    }).catch(e => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибка сервера. Попробуйте снова.'});
    });
});

app.post('/createApplication', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 1) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }

    const { telephone, number, comment } = req.body;
    if(!/^(\d){11}$/.test(telephone) || !/[АВСЕНКМОРТХУ]\d{3}[АВСЕНКМОРТХУ]{2}\d{2,3}$/g.test(number)) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }
    const client = await getClientByTelephone(telephone);
    if(!client) return res.status(404).json({message: 'Клиент с таким номером телефона не найден!'});
    const car = await getCarByNumber(number);
    if(!car) return res.status(404).json({message: 'Автомобиль с таким гос. номером не найден!'});
    createApplication(client["Id"], car["Id"], comment).then((result) => {
        res.status(200).json({ Id: result });
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Заявка не была добавлена. Попробуйте снова.'});
    });
});
app.post('/createClient', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 1) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }

    const {name, telephone, address} = req.body;
    if(name.trim() === "" || !/^(\d){11}$/.test(telephone) || address.trim() === "") {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }
    const client = await getClientByTelephone(telephone);
    if(client) return res.status(302).json({message: 'Клиент с таким номером телефона уже существует!'});
    createClient(name, telephone, address).then(() => {
        res.sendStatus(200);
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Клиент не был добавлен. Попробуйте снова.'});
    });
});
app.post('/createCar', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 1) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }

    const { number, mark, model, engine, body } = req.body;
    if(!/[АВСЕНКМОРТХУ]\d{3}[АВСЕНКМОРТХУ]{2}\d{2,3}$/g.test(number) || engine.length !== 17 ||
        mark.trim() === "" || model.trim() === "" ||
        engine.trim() === "" || body.trim() === "") {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }
    const car = await getCarByNumber(number);
    if(car) return res.status(302).json({message: 'Автомобиль с таким государственным номером уже существует!'});
    createCar(number, mark, model, engine, body).then(() => {
        res.sendStatus(200);
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Автомобиль не был добавлен. Попробуйте снова.'});
    });
});
app.post('/createRepair', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 1) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }

    const { workerId, id } = req.body;
    if(workerId < 1 || id < 1) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }
    const application = await getApplicationById(id);
    if(!application) return res.status(404).json({message: 'Такой заявки не существует!'});
    const worker = (await getAllWorkers()).find(e => e['Id'] === workerId);
    if(!worker) return res.status(404).json({message: 'Такого механика нет.'});

    createRepair(workerId, id).then((result) => {
        res.status(200).json({Id: result});
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Механик не был назначен.'});
    });
});
app.post('/addSparePartRepair', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 1) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }

    const { partId, quantity ,repairId } = req.body;
    if(quantity <= 0 || partId < 1 || repairId < 1) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }

    addSparePartRepair(partId, quantity ,repairId).then((result) => {
        res.sendStatus(200);
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибки сервера при поиске данных.'});
    });
});
app.post('/addService', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 2) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }

    const { serviceId, repairId } = req.body;
    if(serviceId < 1 || repairId < 1) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }

    addService(repairId, serviceId).then((result) => {
        res.sendStatus(200);
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибки сервера при поиске данных.'});
    });
});
app.post('/addDefects', async (req, res) => {
    if(!req.session.sid || !Session.isAuth(req.session.sid)) {
        return res.status(401).json({message: 'Вы не авторизованы!'});
    }
    if(Session.getUser(req.session.sid)?.Id_posts !== 2) {
        return res.status(403).json({message: 'У вас нет прав доступа к данной операции'});
    }

    const { repairId, defects } = req.body;
    if(defects.trim() == "" || repairId < 1) {
        return res.status(400).json({message: 'Неккоректный ввод данных. Попробуйте снова.'});
    }

    addDefects(repairId, defects).then((result) => {
        res.sendStatus(200);
    }).catch((e) => {
        console.log(`[MySQL] ${e}`);
        res.status(500).json({message: 'Ошибки сервера при добавлении данных.'});
    });
});


// Mysql request
function getAllApplication() {
    return new Promise((resolve, reject) => {
        const sql = "SELECT a.Id, cl.Full_name, cl.Telephone, CONCAT(c.Mark, ' ', c.Model) as Car, c.Number, a.Date_start, " +
            "a.Date_end, a.Comments as Comment, s.Name as Status FROM `applications` a, `clients` cl, `cars` c, `status` s " +
            "WHERE a.Id_client = cl.Id AND a.Id_status = s.Id AND a.Id_car = c.Id";
        db.query(sql, [], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getAllStatus() {
    return new Promise((resolve, reject) => {
        const sql = "SELECT * from `status`";
        db.query(sql, [], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getAllCategory() {
    return new Promise((resolve, reject) => {
        const sql = "SELECT * from `service_category`";
        db.query(sql, [], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getAllWorkers() {
    return new Promise((resolve, reject) => {
        const sql = "SELECT w.Id, w.Full_name from `workers` w, `posts` p where w.Id_posts = p.Id AND p.Name = 'Механик'";
        db.query(sql, [], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getAllSpareParts() {
    return new Promise((resolve, reject) => {
        const sql = "SELECT * from `spare_parts`";
        db.query(sql, [], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getApplicationForMechanic(myId) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT a.Id, cl.Full_name, cl.Telephone, CONCAT(c.Mark, ' ', c.Model) as Car, c.Number, a.Date_start, a.Date_end, a.Comments as Comment, s.Name as Status " +
        "FROM `applications` a, `clients` cl, `cars` c, `status` s, `repairs` r " +
        "WHERE a.Id_client = cl.Id AND a.Id_status = s.Id AND a.Id_car = c.Id AND r.Id_applications = a.Id AND " +
            "(a.Id_status = 3 OR a.Id_status = 4) AND r.Id_worker = ?";
        db.query(sql, [myId], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}

function setStatus(status, id) {
    return new Promise((resolve, reject) => {
        const sql = "UPDATE `applications` SET `Id_status` = ? WHERE Id = ?";
        db.query(sql, [status, id], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function setDateEnd(id) {
    return new Promise((resolve, reject) => {
        const sql = "UPDATE `applications` SET `Date_end` = DATE(NOW()) WHERE Id = ?";
        db.query(sql, [id], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}

function getApplicationByMonth() {
    return new Promise((resolve, reject) => {
        const sql = "SELECT a.Id, cl.Full_name, cl.Telephone, CONCAT(c.Mark, ' ', c.Model) as Car, c.Number, " +
            "a.Comments as Comment, s.Name as Status FROM `applications` a, `clients` cl, `cars` c, `status` s " +
            "WHERE a.Id_client = cl.Id AND a.Id_status = s.Id AND a.Id_car = c.Id AND " +
            "MONTH(a.Date_start) = MONTH(NOW()) AND YEAR(a.Date_start) = YEAR(NOW())";
        db.query(sql, [], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getApplicationByDate(filter) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT a.Id, cl.Full_name, cl.Telephone, CONCAT(c.Mark, ' ', c.Model) as Car, c.Number, " +
            "a.Comments as Comment, s.Name as Status FROM `applications` a, `clients` cl, `cars` c, `status` s " +
            "WHERE a.Id_client = cl.Id AND a.Id_status = s.Id AND a.Id_car = c.Id AND " +
            "a.Date_start = DATE(?)";
        db.query(sql, [filter], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getApplicationByClient(filter) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT a.Id, cl.Full_name, cl.Telephone, CONCAT(c.Mark, ' ', c.Model) as Car, c.Number, " +
            "a.Comments as Comment, s.Name as Status FROM `applications` a, `clients` cl, `cars` c, `status` s " +
            "WHERE a.Id_client = cl.Id AND a.Id_status = s.Id AND a.Id_car = c.Id AND " +
            "cl.Telephone = ?";
        db.query(sql, [filter], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getApplicationByCar(filter) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT a.Id, cl.Full_name, cl.Telephone, CONCAT(c.Mark, ' ', c.Model) as Car, c.Number, " +
            "a.Comments as Comment, s.Name as Status FROM `applications` a, `clients` cl, `cars` c, `status` s " +
            "WHERE a.Id_client = cl.Id AND a.Id_status = s.Id AND a.Id_car = c.Id AND " +
            "c.Number = ?";
        db.query(sql, [filter], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getApplicationByStatus(filter) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT a.Id, cl.Full_name, cl.Telephone, CONCAT(c.Mark, ' ', c.Model) as Car, c.Number, " +
            "a.Comments as Comment, s.Name as Status FROM `applications` a, `clients` cl, `cars` c, `status` s " +
            "WHERE a.Id_client = cl.Id AND a.Id_status = s.Id AND a.Id_car = c.Id AND " +
            "s.Name = ?";
        db.query(sql, [filter], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getRepairServicesById(repairId) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT sc.Name as Category, s.Name, s.Price FROM `service_category` sc, `services` s, " +
            "completed_services cs WHERE  s.Id_category = sc.Id AND cs.Id_services = s.Id AND cs.Id_repair = ?";
        db.query(sql, [repairId], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getGivenPartById(repairId) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT sp.Name, sp.Price, isp.Quantity FROM `issued_spare_parts` isp, `spare_parts` sp " +
            "WHERE isp.Id_spare_part = sp.Id AND isp.Id_repair = ?";
        db.query(sql, [repairId], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}
function getServiceByCategory(id) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT * from `services` where `Id_category` = ?";
        db.query(sql, [id], function (err, result) {
            if(err) reject(err);
            else resolve(result);
        })
    })
}

function getUserByLogin(login) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT * from `workers` where `Login`=?";
        db.query(sql, [login], function (err, result) {
            if(err) reject(err);
            else resolve(result[0]);
        })
    })
}
function getClientByTelephone(telephone) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT * from `clients` where `Telephone`=?";
        db.query(sql, [telephone], function (err, result) {
            if(err) reject(err);
            else resolve(result[0]);
        })
    })
}
function getCarByNumber(number) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT * from `cars` where `Number`=?";
        db.query(sql, [number], function (err, result) {
            if(err) reject(err);
            else resolve(result[0]);
        })
    })
}
function getApplicationById(id) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT * from `applications` where Id = ?";
        db.query(sql, [id], function (err, result) {
            if(err) reject(err);
            else resolve(result[0]);
        })
    })
}
function getRepairById(id) {
    return new Promise((resolve, reject) => {
        const sql = "SELECT r.Id, w.Full_name, r.Defects from `repairs` r, `workers` w where w.Id = r.Id_worker AND " +
            "r.Id_applications = ?";
        db.query(sql, [id], function (err, result) {
            if(err) reject(err);
            else resolve(result[0]);
        })
    })
}

function createApplication(idClient, idCar, comment) {
    return new Promise((resolve, reject) => {
        const sql = "INSERT INTO `applications`(`Id_client`, `Id_status`, `Id_car`, `Comments`) VALUES (?,?,?,?)";
        db.query(sql, [idClient, 1, idCar, comment], function (err, result) {
            if(err) return reject(err);
            else resolve(result.insertId);
        })
    })
}
function createClient(name, telephone, address) {
    return new Promise((resolve, reject) => {
        const sql = "INSERT INTO `clients`(`Full_name`, `Telephone`, `Address`) VALUES (?, ?, ?)";
        db.query(sql, [name, telephone, address], function (err, result) {
            if(err) return reject(err);
            else resolve(result.insertId);
        })
    })
}
function createCar(number, mark, model, engine, body) {
    return new Promise((resolve, reject) => {
        const sql = "INSERT INTO `cars`(`Number`, `Mark`, `Model`, `Engine`, `VIN`) VALUES (?,?,?,?,?)";
        db.query(sql, [number, mark, model, engine, body], function (err, result) {
            if(err) return reject(err);
            else resolve(result.insertId);
        })
    })
}
function createRepair(workerId, id) {
    return new Promise((resolve, reject) => {
        const sql = "INSERT INTO `repairs`(`Id_worker`, `Id_applications`) VALUES (?,?)";
        db.query(sql, [workerId, id], function (err, result) {
            if(err) return reject(err);
            else resolve(result.insertId);
        })
    })
}
function addSparePartRepair(partId, quantity, repairId) {
    return new Promise((resolve, reject) => {
        const sql = "INSERT INTO `issued_spare_parts`(`Id_repair`, `Id_spare_part`, `Quantity`) VALUES (?,?,?)";
        db.query(sql, [repairId, partId, quantity], function (err, result) {
            if(err) return reject(err);
            else resolve(result.insertId);
        })
    })
}
function addService(repairId, serviceId) {
    return new Promise((resolve, reject) => {
        const sql = "INSERT INTO `completed_services`(`Id_services`, `Id_repair`) VALUES (?, ?)";
        db.query(sql, [serviceId, repairId], function (err, result) {
            if(err) return reject(err);
            else resolve(result.insertId);
        })
    })
}
function addDefects(repairId, defects) {
    return new Promise((resolve, reject) => {
        const sql = "UPDATE `repairs` SET `Defects` = ? WHERE `Id` = ?";
        db.query(sql, [defects, repairId], function (err, result) {
            if(err) return reject(err);
            else resolve(result);
        })
    })
}