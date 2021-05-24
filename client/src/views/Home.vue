<template>
  <v-container fluid>

    <v-overlay :value="overlay">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>

    <v-snackbar content-class="d-flex justify-space-between" v-model="snackbar" :timeout="15000" top
                :color="snackbar_color">
      {{ snackbar_text }}
      <v-icon aria-label="Close" @click="snackbar = false" color="white">
        {{ mdiClose }}
      </v-icon>
    </v-snackbar>

    <v-dialog v-model="delete_dialog" max-width="480">
      <v-card>
        <v-card-title>
          آیا از حذف فایل های انتخاب شده اطمینان دارید؟
          <v-spacer/>
          <v-icon aria-label="Close" @click="delete_dialog = false" color="red">
            {{ mdiClose }}
          </v-icon>
        </v-card-title>
        <v-card-text class="pb-6 pt-12 text-center">
          <v-btn class="mr-3" text @click="delete_dialog = false">
            خیر
          </v-btn>
          <v-btn color="success" text @click="deleteItem">
            بله
          </v-btn>
        </v-card-text>
      </v-card>
    </v-dialog>

    <v-dialog v-model="select_date_dialog" max-width="480">
      <v-card>
        <validation-observer v-slot="{invalid,valid}">
          <v-card-title>
            بازه تاریخ مورد نظر را انتخاب کنید
            <v-spacer/>
            <v-icon aria-label="Close" @click="date_filter" color="green" :disabled="invalid">
              {{ mdiCheck }}
            </v-icon>
          </v-card-title>
          <v-card-text class="pb-6 pt-12 text-center">
            <validation-provider name="from_date" rules="required" v-slot="{errors}">
              <v-text-field v-model="from_date" :prepend-icon="mdiCalendar" @click:prepend="show_from_date=true"
                            maxlength="30" required :error-messages="errors" id="from_date"
                            label="از تاریخ (YYYY-MM-DD HH:mm)"></v-text-field>
              <date-picker v-model="from_date" type="datetime" color="#1e88e5"
                           :max="from_date_max+' 23:59'" format="jYYYY-jMM-jDD HH:mm"
                           display-format="jYYYY-jMM-jDD HH:mm" :editable="true"
                           element="from_date" :show="show_from_date" @close="show_from_date=false"/>
            </validation-provider>
            <validation-provider name="to_date" rules="required" v-slot="{errors}">
              <v-text-field v-model="to_date" :prepend-icon="mdiCalendar" @click:prepend="show_to_date=true"
                            required :error-messages="errors" id="to_date" :disabled="from_date===''" maxlength="30"
                            label="تا تاریخ (YYYY-MM-DD HH:mm)"></v-text-field>
              <date-picker v-model="to_date" type="datetime" color="#1e88e5" :editable="true"
                           :min="from_date" :max="to_date_max" format="jYYYY-jMM-jDD HH:mm"
                           display-format="jYYYY-jMM-jDD HH:mm" :disabled="from_date===''"
                           element="to_date" :show="show_to_date" @close="show_to_date=false"/>
            </validation-provider>
            <v-select :items="folders" label="انتخاب پوشه" item-text="name" item-value="id"
                      v-model="selected_folder"></v-select>
          </v-card-text>
        </validation-observer>
      </v-card>
    </v-dialog>

    <v-card>
      <v-card-title>
        <v-menu offset-y>
          <template v-slot:activator="{ on, attrs }">
            <v-chip class="mx-2" color="red" outlined v-bind="attrs" v-on="on">{{ filter_types_title }}</v-chip>
          </template>
          <v-list>
            <v-list-item v-for="(item, index) in file_types" :key="index"
                         @click="filter_types(item)">
              <v-list-item-title>{{ item.title }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
        <v-chip v-if="selected.length>0" class="mx-2" color="green" outlined>فایل های انتخاب شده : {{ selected.length }}
        </v-chip>
        <v-chip
            v-if="selected.length>0"
            dir="ltr"
            class="mx-2"
            color="orange"
            outlined>{{ total_size ? `${total_size} KB` : '0 KB' }}
        </v-chip>
        <v-chip
            @click="select_date_dialog=true"
            class="mx-2"
            color="primary"
            outlined>
          {{
            from_date || to_date ? from_date.substring(0, 10) + ' - ' + to_date.substring(0, 10) : '0000-00-00 - 0000-00-00'
          }}
        </v-chip>
        <v-text-field outlined label="جستجو" single-line hide-details class="mx-2" dense color="info"
                      @keyup="start_search" @keydown="stop_search"></v-text-field>
        <v-chip
            class="mx-2"
            color="blue"
            outlined>تعداد فایل ها : {{ files_count }}
        </v-chip>
        <v-btn icon class="mx-2 white--text" color="blue" rounded @click="page-=1" outlined :disabled="page===1">
          <v-icon>{{ mdiArrowRight }}</v-icon>
        </v-btn>
        <v-btn icon class="mx-2 white--text" color="blue" rounded @click="page+=1" outlined>
          <v-icon>{{ mdiArrowLeft }}</v-icon>
        </v-btn>
        <v-btn class="mx-2 white--text" color="red" @click.stop="delete_dialog=true" rounded icon outlined
               :disabled="!items.length>0 || !selected.length>0">
          <v-icon>{{ mdiDelete }}</v-icon>
        </v-btn>
        <v-btn icon class="mx-2 white--text" color="success" rounded @click.stop="date_filter" outlined>
          <v-icon>{{ mdiRefresh }}</v-icon>
        </v-btn>
        <v-btn icon class="mx-2 white--text" color="red" rounded @click.stop="logout" outlined>
          <v-icon>{{ mdiLogout }}</v-icon>
        </v-btn>
      </v-card-title>
      <v-data-table
          show-select
          selectable-key="id"
          no-results-text="هیچ اطلاعاتی یافت نشد"
          no-data-text="هیچ اطلاعاتی یافت نشد"
          :loading-text="loading_text"
          v-model="selected"
          :headers="headers"
          :items="items"
          :loading="loading"
          :search="search"
          :items-per-page="items_per_page"
          :page.sync="page"
          :footer-props="{
          itemsPerPageOptions:[50,100,200,files_count].sort(function(a, b){return a - b})
          }"
      >
        <v-progress-linear style="direction: ltr !important;" v-model="progress_value" :active="loading"
                           :indeterminate="progress_indeter" color="blue"
                           slot="progress"
                           :query="true"></v-progress-linear>

        <template v-slot:item.folder_id="{ item }">
          <td v-if="item.folder_id" style="direction: ltr;float: left">{{ item.folder_id }}</td>
          <td v-else>
            <v-btn
                text
                class="ma-2"
                :loading="item.loading"
                :disabled="item.loading"
                color="primary"
                @click="load_path(item)">
              دریافت مسیر
            </v-btn>
          </td>
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script>
import
{
  mdiClose,
  mdiDelete,
  mdiLogout,
  mdiRefresh,
  mdiDeleteForever, mdiCheck, mdiCalendar, mdiArrowRight, mdiArrowLeft
} from '@mdi/js';
import * as signalR from '@microsoft/signalr'

let socket;
export default {
  data() {
    return {
      filter_types_title: 'فیلتر نوع',
      file_types: [
        {id: 0, title: 'هیچ کدام'},
        {id: 1, title: 'archive'},
        {id: 2, title: 'attachment'},
        {id: 3, title: 'content'},
        {id: 4, title: 'flv'},
        {id: 5, title: 'image'},
        {id: 6, title: 'mp3'},
        {id: 7, title: 'mp4'},
        {id: 8, title: 'pdf'},
        {id: 9, title: 'pdf2swf'},
        {id: 10, title: 'producer-hybrid'},
        {id: 11, title: 'swf'},
      ],
      typing_timer: null,
      page: 1,
      items_per_page: 50,
      loading: false,
      progress_indeter: true,
      progress_value: 0,
      folder_sco_id: null,
      selected_folder: 0,
      folders: [],
      loading_text: "در حال دریافت لیست فایل ها ...",
      files_count: 0,
      url: 'Adobe connect',
      show_from_date: false,
      show_to_date: false,
      from_date_max: this.$moment().locale('fa').subtract(1, 'day').format('YYYY-MM-DD'),
      to_date_max: this.$moment().locale('fa').format('YYYY-MM-DD'),
      from_date: '',
      to_date: '',
      select_date_dialog: false,
      search: '',
      snackbar_color: 'success',
      snackbar: false,
      snackbar_text: 'عملیات با موفقیت انجام شد',
      selected: [],
      delete_dialog: false, mdiArrowRight, mdiArrowLeft,
      mdiClose, mdiDelete, mdiDeleteForever, mdiCheck, mdiCalendar, mdiLogout,
      mdiRefresh,
      overlay: false,
      items: [],
      items_default: [],
      headers: [
        {text: 'شناسه', align: 'center', sortable: true, value: 'id', 'class': 'primary--text'},
        {text: 'نام', align: 'center', sortable: true, value: 'title', 'class': 'primary--text'},
        {text: 'نوع', align: 'center', sortable: true, value: 'type', 'class': 'primary--text'},
        {text: 'حجم (KB)', align: 'center', sortable: true, value: 'size', 'class': 'primary--text'},
        {
          text: 'مسیر',
          align: 'start',
          sortable: true,
          value: 'folder_id',
          class: 'primary--text',
        },
        {text: 'تاریخ', align: 'center', sortable: true, value: 'date', 'class': 'primary--text'},
      ],
    }
  },
  computed: {
    total_size() {
      return this.selected.map(item => item.size).reduce((total, size) => {
        return parseInt(total) + parseInt(size)
      }, 0)
    }
  },
  beforeCreate() {
    socket = new signalR.HubConnectionBuilder()
        .withUrl(`/adobe`, {
          // .withUrl(`http://${location.hostname}:5000/adobe`, {
          // skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets,
        }).configureLogging(signalR.LogLevel.Error)
        .withAutomaticReconnect()
        .build();
  },
  created() {
    socket.start().then(() => {
      this.overlay = true
      if (this.$cookie.get('sco_id') && this.$cookie.get('meeting_url') && this.$cookie.get('BREEZETOKEN'))
        socket.invoke('Folders', this.$cookie.get('BREEZETOKEN'), this.$cookie.get('meeting_url'), this.$cookie.get('sco_id'))
      else
        this.$router.push('/login')
    }).catch(err => console.log(err));
  },
  mounted() {
    this.url = this.$cookie.get('meeting_url')
    document.title = this.url
    socket.on('Folders', (data) => {
      this.overlay = false
      this.select_date_dialog = true
      if (!data.error) {
        this.folders = [{id: 0, name: 'همه موارد'}, ...data]
      } else {
        if (data.error === 'no-login') {
          this.$router.push('/login')
        } else {
          this.snackbar_color = 'error';
          this.snackbar_text = data.error;
          this.snackbar = true;
        }
      }
    })
    socket.on('ScoDelete', (data) => {
      this.loading = false
      this.progress_value = 0
      this.progress_indeter = false
      if (data.error) {
        this.snackbar_color = 'error';
        this.snackbar_text = data.error;
        this.snackbar = true;
      } else {
        this.items = this.items.filter(i => !data.includes(i.id))
        this.files_count -= this.selected.length
        this.selected = [];
      }
    })
    socket.on('GetScoExpandedContents', (data) => {
      this.loading = false
      if (data.error) {
        if (data.error === 'no-login') {
          this.$router.push('/login')
        } else {
          alert(data.error)
        }
      } else {
        this.items_default = this.items = data
        // this.headers.find(h => h.value === 'folder_id').text = 'مسیر (در حال دریافت)'
        this.progress_indeter = false
      }
    })
    socket.on('ShowProgress', (data) => {
      this.progress_indeter = false
      this.progress_value = data
    })
    socket.on('FilesCount', (data) => {
      data ? this.files_count = data : this.files_count = 0
      this.loading_text = 'در حال دریافت حجم فایل ها ...'
    })
    socket.on('FilesPath', (data) => {
      this.items.find(i => i.id === data.id).folder_id = data.path
      if (!this.items.some(i => !i.folder_id)) {
        this.headers.find(h => h.value === 'folder_id').text = 'مسیر'
      }
    })
    socket.on('FilePath', (data) => {
      const it = this.items.find(i => i.id === data.id)
      it.folder_id = data.path
      it.loading = false
    })
  },
  methods: {
    filter_types(type) {
      if (type.id === 0) {
        this.filter_types_title = 'فیلتر نوع'
        this.items = this.items_default
      } else {
        this.filter_types_title = type.title
        this.items = this.items_default.filter(i => i.type === type.title)
      }
    },
    start_search(e) {
      clearTimeout(this.typing_timer)
      this.typing_timer = setTimeout(() => {
        this.search = e.target.value
      }, 1000)
    },
    stop_search() {
      clearTimeout(this.typing_timer)
    },
    load_path(item) {
      item.loading = true
      socket.invoke('FilePath', {
        depth: item.depth,
        id: item.id,
        session: this.$cookie.get('BREEZETOKEN'),
        url: this.$cookie.get('meeting_url')
      })
    },
    logout() {
      this.$cookie.delete('BREEZETOKEN');
      this.$cookie.delete('meeting_url');
      this.$cookie.delete('sco_id');
      document.title = 'ADOBE MANAGER'
      this.$router.push('/login')
    },
    date_filter() {
      this.selected = [];
      this.select_date_dialog = false
      this.progress_value = 0
      const from = this.$moment(this.from_date, 'jYYYY-jMM-jDD HH:mm').format('YYYY-MM-DD')
      const to = this.$moment(this.to_date, 'jYYYY-jMM-jDD HH:mm').format('YYYY-MM-DD')
      this.getFiles(from, to)
    },
    getFiles(from, to) {
      this.loading = true;
      socket.invoke('GetScoExpandedContents', this.$cookie.get('BREEZETOKEN'), this.$cookie.get('meeting_url'), this.selected_folder === 0 ? this.$cookie.get('sco_id') : this.selected_folder, from, to)
    },
    deleteItem() {
      this.delete_dialog = false;
      this.loading = true
      this.progress_indeter = false
      this.progress_value = 0
      const ids = this.selected.map(item => item.id)
      socket.invoke('ScoDelete', {
        ids: ids,
        session: this.$cookie.get('BREEZETOKEN'),
        url: this.$cookie.get('meeting_url')
      })
    },
  },
}
</script>
<style scoped>
table td {
  direction: ltr !important;
}
</style>
