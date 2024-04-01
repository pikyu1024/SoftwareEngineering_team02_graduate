namespace project_manage_system_backend.Dtos
{

    public class TrelloOutboundAddListDto
    {
        /// <summary>
        /// List名稱，因為trello api要求小寫，所以這裡必須小寫
        /// </summary>
        public string name { get; set; }
        public bool value { get; set; }
    }
}
