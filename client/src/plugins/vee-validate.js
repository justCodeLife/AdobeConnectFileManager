import Vue from 'vue'
import {
  extend,
  ValidationObserver,
  ValidationProvider,
} from 'vee-validate'
import {
  email,
  max,
  min,
  numeric,
  required,
  required_if,
  alpha,
  alpha_dash,
  alpha_num,
  alpha_spaces,
  regex
} from 'vee-validate/dist/rules'

extend('email', {...email, message: 'ایمیل نامعتبر می باشد'})
extend('max', {...max, message: 'تعداد حروف این فیلد بیش از حد مجاز است'})
extend('min', {...min, message: 'تعداد حروف این فیلد کمتر از حد مجاز است'})
extend('numeric', {...numeric, message: 'این فیلد باید به صورت عدد باشد'})
extend('required', {...required, message: 'لطفا این فیلد را وارد نمایید'})
extend('required_if', {...required_if, message: 'لطفا این فیلد را وارد نمایید'})
extend('alpha', {...alpha, message: 'این فیلد فقط می تواند شامل حروف الفبا باشد'})
extend('alpha_num', {...alpha_num, message: 'این فیلد فقط می تواند شامل حروف الفبا یا عدد باشد'})
extend('alpha_dash', {...alpha_dash, message: 'این فیلد فقط می تواند شامل حروف الفبا یا عدد یا خط تیره یا زیرخط باشد'})
extend('alpha_spaces', {...alpha_spaces, message: 'این فیلد فقط می تواند شامل حروف الفبا یا فاصله باشد'})
extend('regex', {...regex, message: 'این فیلد نامعتبر است'})
extend('is_num_or_email', {
  validate(value, args) {
    const email_regex = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    const mobile_regex = /^(?:0|98|\+98|\+980|0098)?(9\d{9})$/
    return email_regex.test(value.toString().toLowerCase()) || mobile_regex.test(value);
  }, message: 'ایمیل یا شماره موبایل نامعتبر است'
})

Vue.component('validation-provider', ValidationProvider)
Vue.component('validation-observer', ValidationObserver)
