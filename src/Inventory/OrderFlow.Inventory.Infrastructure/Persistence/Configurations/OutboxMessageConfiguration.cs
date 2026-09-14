using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Infrastructure.Persistence.Messaging;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(message => message.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.HasIndex(message => message.EventId)
            .IsUnique();

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
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(message => message.PublishedAt)
            .HasColumnName("published_at");
    }
}
