namespace AWS_ClassLibrary;

public record Order(Guid PedidoId, Guid ClienteId, DateTimeOffset Fecha, decimal Total);
