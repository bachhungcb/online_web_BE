using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataAccess.EFCore.Context;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Friend> Friends { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Conversation> Conversation { get; set; }


    public async Task<int> SaveChanges()
    {
        return await base.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình Conversation entity
        modelBuilder.Entity<Conversation>(builder =>
        {
            // ▼▼▼ THÊM CẤU HÌNH NÀY ▼▼▼

            // Cấu hình Participants: List<Guid> <-> JSON String
            builder.Property(c => c.Participants)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null), // Convert to DB
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null) ??
                         new List<Guid>() // Convert from DB
                )
                .Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // Cấu hình SeenBy: List<Guid> <-> JSON String (Tương tự)
            builder.Property(c => c.SeenBy)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null) ?? new List<Guid>()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

            // ▲▲▲ KẾT THÚC CẤU HÌNH ▲▲▲

            // Định nghĩa rằng 'Group' là một owned entity
            // và dữ liệu của nó sẽ được lưu trong cùng bảng với Conversation
            builder.OwnsOne(conversation => conversation.Group, ownedNavigationBuilder =>
            {
                // Bạn có thể tùy chỉnh tên cột nếu muốn
                ownedNavigationBuilder.Property(groupInfo => groupInfo.Name)
                    .HasColumnName("GroupName"); // Cột trong DB sẽ là 'GroupName'
                ownedNavigationBuilder.Property(groupInfo => groupInfo.GroupAvatar)
                    .HasColumnName("GroupAvatar");
                ownedNavigationBuilder.Property(groupInfo => groupInfo.CreatedBy) // Sửa lỗi chính tả từ CreadtedBy
                    .HasColumnName("GroupCreatedBy"); // Cột trong DB sẽ là 'GroupCreatedBy'
            });

            // --- THÊM MỚI: Cấu hình cho LastMessage ---
            builder.OwnsOne(conversation => conversation.LastMessage, lastMessageBuilder =>
            {
                // LastMessage cũng có thể là null khi cuộc trò chuyện mới được tạo

                lastMessageBuilder.Property(messageInfo => messageInfo.Content)
                    .HasColumnName("LastMessageContent"); // Đặt tên cột rõ ràng

                lastMessageBuilder.Property(messageInfo => messageInfo.CreatedAt)
                    .HasColumnName(
                        "LastMessageCreatedAt"); // Rất quan trọng để tránh xung đột với cột 'CreatedAt' của Conversation

                lastMessageBuilder.Property(messageInfo => messageInfo.Sender)
                    .HasColumnName("LastMessageSender");

                lastMessageBuilder.Property(messageInfo => messageInfo.MessageType)
                    .HasColumnName("LastMessageType");
            });
        });

        modelBuilder.Entity<FriendRequest>(entity =>
        {
            // Mối quan hệ "Người gửi"
            entity.HasOne(fr => fr.Sender)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ "Người nhận"
            entity.HasOne(fr => fr.Receiver)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Friend>(entity =>
        {
            // Mối quan hệ khi User này là người chủ động kết bạn (UserA)
            entity.HasOne(f => f.FriendA)
                .WithMany(u => u.Friendships) // <-- Trỏ đến collection 1
                .HasForeignKey(f => f.UserA)
                .OnDelete(DeleteBehavior.Restrict);

            // Mối quan hệ khi User này là người được kết bạn (UserB)
            entity.HasOne(f => f.FriendB)
                .WithMany(u => u.FriendedBy) // <-- Trỏ đến collection 2
                .HasForeignKey(f => f.UserB)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(builder =>
        {
            // Cấu hình MediaUrls: List<string> <-> JSON String
            builder.Property(m => m.MediaUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null), // Chiều ghi vào DB (giữ nguyên)
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>() // <--- FIX: Nếu DB rỗng thì trả về List rỗng, KHÔNG Deserialize
                        : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        });
        modelBuilder.Entity<MessageReaction>(entity =>
        {
            // Composite Key (Optional): Để đảm bảo 1 User chỉ thả 1 loại reaction cho 1 Message
            // Nhưng BaseEntity đã có Id riêng nên ta dùng Index để tối ưu query
            entity.HasIndex(r => new { r.MessageId, r.UserId });

            entity.HasOne(r => r.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa tin nhắn -> Xóa luôn reaction

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}