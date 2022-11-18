using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BottApp.Database.Document.Like;

public class LikedDocumentModel : AbstractModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int DocumentId { get; set; }
    
    [ForeignKey("DocumentId")]
    public DocumentModel DocumentModel { get; set; }

    public bool isLiked { get; set; }
    
}