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

            builder.Property(message => message.CreateAt)
                .HasColumnName("create_at")
                .IsRequired();

            builder.Property(message => message.PublishAt)
                .HasColumnName("publish_at")
                .IsRequired();
        }
    }
}