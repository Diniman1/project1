function Session() {
    this.auth = false;
    this.setSession = function (session) {
        this.session = session;
    }
    this.isAuth = function () {
        return this.auth;
    }
    this.authorize = function (user) {
        this.auth = true;
        this.user = user;
    }
}
const sessions = {};

function add(session) {
    sessions[session.sid] = new Session();
    sessions[session.sid].setSession(session);
}
function isAuth(sid) {
    return sessions[sid].isAuth();
}
function authorize(sid, user) {
    sessions[sid].authorize(user);
}
function logout(sid) {
    delete sessions[sid];
}
function getUser(sid) {
    return sessions[sid].user;
}

module.exports = {add, isAuth, authorize, getUser, logout}