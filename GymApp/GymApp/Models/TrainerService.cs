using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Models
{
    public class TrainerService
    {
        [Key]
        public int Id { get; set; }

        public int TrainerId { get; set; }
        [ForeignKey("TrainerId")]
        public Trainer Trainer { get; set; }

        public int GymServiceId { get; set; }
        [ForeignKey("GymServiceId")]
        public GymService GymService { get; set; }
    }
}