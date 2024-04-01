<template>
  <div class="border card-width">
    <el-row>
      <v-edit-dialog large save-text="新增" @save="addNewList" cancel-text="取消">
        <div class="text-h8 el-icon-plus add-card">新增欄位</div>
        <template v-slot:input>
          <v-text-field label="欄位名稱" v-model="newListName"></v-text-field>
        </template>
      </v-edit-dialog>
    </el-row>
  </div>

</template>

<script lang="ts">
import Vue from "vue";
import { addNewListToBoard } from "@/apis/trello";
// import Box from "@/components/Box.vue";

declare interface Data {
  id: string;
  name: string;
}

declare interface List {
  listId: string;
  name: string;
}

export default Vue.extend({
  props: {
    BoardId: String,
    opencard: {
      type: Object,
      default: () => ({}),
    },
  },
  data() {
    return {
      cardList: [] as Data[],
      optionList: [] as List[],
      newCardName: "",
      newListName: "",
      dialogVisible: false,
      form: { region: "" },
      textarea: "",
      selection: "",
      name: "",
      currentCardId: "",
      // cardData:[{name:,data:}]
    };
  },
  //   mounted() {},
  methods: {
    addNewList() {
      this.addAsyncNewListToBoard();
      this.newListName = "";
    },

    async addAsyncNewListToBoard() {
      const res = await addNewListToBoard(
        "123",
        this.BoardId,
        this.newListName
      );
      if (res) {
        console.log(this.$refs["dialog"]);
        this.$emit("created");
      }
    },
  },
});
</script>

<style lang="scss">
.border {
  border-width: 1px;
  border-style: solid;
  border-color: rgb(166, 177, 177);
  padding: 5px;
  border-radius: 3px;
  margin: 10px;
}

.action {
  width: 70%;
  border-radius: 5px;
}

.action:hover {
  background-color: rgba(76, 83, 88, 0.199);
}

.card-width {
  width: 300px;
}

.card-style {
  margin-top: 10px;
}

.card-style {
  cursor: pointer;
}

.add-card {
  margin-top: 15px;
  width: 100%;
  height: 25px;
  border-radius: 5px;
  padding-top: 5px;
}

.add-card:hover {
  background-color: rgba(76, 83, 88, 0.199);
  cursor: pointer;
}
</style>
