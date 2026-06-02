public class StudentAttachment
{
    public int AttachmentId { get; set; }
    public int StudentId { get; set; }
    public string FileName { get; set; }
    public byte[] FileData { get; set; }
    public DateTime UploadDate { get; set; }
}