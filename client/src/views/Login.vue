<template>
  <v-app id="inspire">
    <v-content>
      <v-container fluid fill-height>
        <v-layout align-center justify-center>
          <v-flex xs12 sm8 md4>
            <v-card class="elevation-12">
              <validation-observer ref="form" v-slot="{invalid,valid}">
                <v-form @submit.prevent="submit">
                  <v-toolbar dark color="primary" class="d-flex justify-center">
                    <v-toolbar-title>سیستم مدیریت فایل Adobe connect</v-toolbar-title>
                  </v-toolbar>
                  <v-card-text class="px-5">
                    <validation-provider name="url" rules="required" v-slot="{errors}">
                      <v-text-field v-model="url" required label="آدرس (مثال : http://meeting.viannacloud.ir)"
                                    type="text"
                                    :error-messages="errors"
                                    maxlength="50"></v-text-field>
                    </validation-provider>
                    <validation-provider name="email" rules="required|email" v-slot="{errors}">
                      <v-text-field v-model="email" required label="ایمیل" type="email" :error-messages="errors"
                                    maxlength="50"></v-text-field>
                    </validation-provider>
                    <validation-provider name="password" rules="required" v-slot="{errors}">
                      <v-text-field v-model="password" required label="رمز عبور" type="password"
                                    :error-messages="errors"
                                    maxlength="50"></v-text-field>
                    </validation-provider>
                  </v-card-text>
                  <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="primary" type="submit" :disabled="invalid">ورود</v-btn>
                  </v-card-actions>
                </v-form>
              </validation-observer>
            </v-card>
          </v-flex>
        </v-layout>
      </v-container>
    </v-content>
  </v-app>
</template>

<script>

export default {
  name: 'Login',
  data() {
    return {
      url: '',
      email: '',
      password: '',
    }
  },
  mounted() {
    this.$cookie.delete('BREEZETOKEN');
    this.$cookie.delete('meeting_url');
    this.$cookie.delete('sco_id');
  },
  methods: {
    async submit() {
      if (!this.url.startsWith('http://') && !this.url.startsWith('https://')) {
        alert('آدرس باید با https یا http شروع شود')
        return
      }
      try {
        const res = await this.$http.get(`/login`, {
          params: {
            url: this.url, email: this.email, password: this.password
          }
        })
        if (res.data.success) {
          this.$cookie.set('BREEZETOKEN', res.data.token, {expires: '30m'})
          const r = await this.$http.get('/sco_shortcuts', {
            params: {
              session: res.data.token, url: this.url
            }
          })
          if (r.data.error) {
            if (r.data.error === 'no-login') {
              this.$router.push('/login')
            }
            alert(r.data.error)
          } else {
            this.$cookie.set('sco_id', r.data, {expires: '5h'})
            this.$cookie.set('meeting_url', this.url, {expires: '5h'})
            document.title = this.url
            this.$router.push('/')
          }
        } else {
          alert(res.data.error)
        }
      } catch (e) {
        alert('ورود با خطا مواجه شد')
        console.log(e)
      }
    }
  }
};
</script>
