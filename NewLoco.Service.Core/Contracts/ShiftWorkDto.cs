namespace NewLoco.Service.Core.Contracts
    {
    public class ShiftWorkDto
        {
        public int Id { get; set; }
        public int LocoId { get; set; }
        public DateTime Date { get; set; }
        public decimal FinalValue { get; set; }
        }
    }
