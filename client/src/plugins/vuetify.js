import Vue from 'vue';
import Vuetify from 'vuetify';
import 'vuetify/dist/vuetify.min.css';
import fa from 'vuetify/lib/locale/fa';

Vue.use(Vuetify);

export default new Vuetify({
  rtl: true,
  theme: {
      options: {
        customProperties: true,
      },
    themes: {
      light: {
        primaryinfo: '#ee44aa',
        secondary: '#424242',
        accent: '#82B1FF',
        error: '#FF5252',
        primary: '#2196F3',
        success: '#4CAF50',
        warning: '#FFC107'
      },
    },
  },
    lang: {
      locales: { fa },
      current: 'fa',
    },
});
