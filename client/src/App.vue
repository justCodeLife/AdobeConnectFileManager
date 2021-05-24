<template>
  <v-app>
    <router-view/>
  </v-app>
</template>

<script>

export default {
  name: 'App',
  beforeDestroy() {
    this.$cookie.delete('BREEZETOKEN');
    this.$cookie.delete('meeting_url');
    this.$cookie.delete('sco_id');
  }
};
</script>
