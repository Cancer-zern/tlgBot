namespace tlgBotConsole
{
    class EmailForm
    {
        public string TicketType { get; set; }
        public string TicketCategory { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public int TicketNumber { get; set; }
        public void Clear()
        {
            this.TicketType = null;
            this.TicketCategory = null;
            this.UserName = null;
            this.PhoneNumber = null;
            this.TicketNumber = -1;
        }

    }
}
