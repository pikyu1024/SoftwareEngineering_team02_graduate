import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import VCharts from "v-charts";
import Vuetify from "vuetify";
import Element from "element-ui";
import "vuetify/dist/vuetify.min.css";

Vue.use(Vuetify);
Vue.use(VCharts);
Vue.use(Element);

Vue.config.productionTip = false;

new Vue({
  vuetify: new Vuetify(),
  router,
  render: (h) => h(App),
}).$mount("#app");
