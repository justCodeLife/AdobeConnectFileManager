import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import vuetify from './plugins/vuetify';
import "./plugins/vee-validate";
import axios from 'axios'
import VueAxios from 'vue-axios'
import VueCookie from 'vue-cookie';
import VuePersianDatetimePicker from 'vue-persian-datetime-picker';

// development
// axios.defaults.baseURL = `http://${location.hostname}:5000/api`;
// production
axios.defaults.baseURL = '/api';
axios.defaults.timeout = 0
Vue.component('date-picker', VuePersianDatetimePicker);
Vue.use(require('vue-jalali-moment'));
Vue.use(VueAxios, axios)
Vue.config.productionTip = false
Vue.use(VueCookie);
new Vue({
    router,
    store,
    vuetify,
    render: function (h) {
        return h(App)
    }
}).$mount('#app')
