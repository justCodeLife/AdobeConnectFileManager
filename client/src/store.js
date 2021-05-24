import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
    state: {
        sco_id: null,
        email: "",
        password: "",
        url: ''
    },
    mutations: {
        set_sco_id(state, payload) {
            state.sco_id = payload
        },
        set_email(state, payload) {
            state.email = payload
        },
        set_password(state, payload) {
            state.password = payload
        },
        set_url(state, payload) {
            state.url = payload
        },
    },
    getters: {
        sco_id: state => state.sco_id,
        email: state => state.email,
        password: state => state.password,
        url: state => state.url,
    }
})
