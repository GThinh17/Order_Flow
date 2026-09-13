using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Orders.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Orders.Infrastructure.Persistence.Configuration
{
    public sealed class OutboxMessageConfiguration
        : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(
            EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("out_messages");

            builder.HasKey(messsage => messsage.Id);

            builder.Property(message => message.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(message => message.EventId)
                .HasColumnName("event_id")
                .IsRequired();

            builder.Property(message => message.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(message => message.Topic)
                .HasColumnName("topic")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(message => message.PartitionKey)
                .HasColumnName("partition_key")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(message => message.Payload)
                .HasColumnName("payload")
                .HasColumnType("json")
                .IsRequired();

            builder.Property(message => message.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(message => message.PublishedAt)
                .HasColumnName("published_at");
        }
    }
}
