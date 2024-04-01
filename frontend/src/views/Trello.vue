<template>
  <v-container fill-height fluid class="align-center justify-center paper">
    <TrelloList
      :status="status"
      :ListName="index.name"
      :ListId="index.listId"
      v-for="(index) in dataList"
      :key="index.listId"
      class="list"
      @on-change-start="emitEventStart"
      @on-change-end="emitEventEnd"
      @delete="removeList"
    ></TrelloList>
    <!-- <TrelloListAdd :status="status" v-for="index in boardList" :key="index.boardId" class="board"
            @on-change-start="emitEventStart" @on-change-end="emitEventEnd"></TrelloListAdd> -->
    <TrelloListAdd :BoardId="boardId" class="list" @created="init">
    </TrelloListAdd>
  </v-container>
  <!-- 在這邊寫一個新增list的按鈕 -->
  <!-- <v-container class="paper">
        <TrelloListAdd :status="status" :ListName="index.name" :ListId="index.listId"  v-for="index in dataList" :key="index.listId" class="list" @on-change-start="emitEventStart" @on-change-end="emitEventEnd"></TrelloListAdd>
    </v-container> -->
</template>

<script lang="ts">
import Vue from "vue";
import TrelloList from "@/components/TrelloList.vue";
import TrelloListAdd from "@/components/TrelloListAdd.vue";
import { getTrelloListById } from "@/apis/trello";
// import { getTrelloBoardById } from "@/apis/trello";
//import Box from "@/components/Box.vue"

declare interface List {
  listId: string;
  name: string;
}

declare interface Board {
  boardId: string;
}

export default Vue.extend({
  components: {
    TrelloList,
    TrelloListAdd,
    //Box
  },
  computed: {
    boardId() {
      console.log(this.$route);
      return this.$route.params.boardId;
    },
  },
  data() {
    return {
      //dataList: [{ type: Object, id: "", name: ""}],
      // dataList: new Array<Data>(),
      dataList: [] as List[],
      // boardList: [] as Board[],
      show: false,
      modalClass: "",
      dialogVisible: false,
      status: "",
    };
  },
  mounted() {
    this.init();
  },
  methods: {
    removeList(id: string) {
        this.dataList = this.dataList.filter((item) => item.listId !== id);
    },
    init() {
      this.getAsyncTrelloListById();
    },
    emitEventStart(ListId: string) {
      this.status = ListId;
    },
    emitEventEnd(recv: string) {
      this.status = recv;
    },
    async getAsyncTrelloListById() {
      const dataList = [];
      const responses = await getTrelloListById(
        "123",
        this.$route.params.boardId
      );
      for (const response in responses.data) {
        dataList.push({
          listId: responses.data[response].listId,
          name: responses.data[response].listName,
        });
      }
      this.dataList = [...dataList];
    },
  },
});
</script>

<style lang="scss">
.paper {
  background-color: rgb(255, 255, 255) !important;
  display: block !important;
}

.list {
  display: inline-grid;
}

.board {
  display: grid;
}
</style>
