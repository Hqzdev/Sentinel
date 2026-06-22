// тут храним снимок оперативныой памяти в момент времени

namespace SystemMonitor.Core.Models;

// все знаяние идут в байтах внутри но будет отображать в mb gb
public sealed record RamSnapshot
{
    // вот тут момент ремени коггла были созданы метрики
    public DateTime Timestamp { get; init;}
    // общий обьем физической памятм в байтах
    // пример 17_179_869_184 для 16 гб
    // формула - кол во гб * 1024 * 1024 * 1024
    // long потому что может не влезить в int
    public long TotalBytes { get; init;}
    // сколько байт сейчас используется
    public long UsedBytes { get; init;}
    // сколько байт свободно
    // можно вычислить как TotalBytes - UsedBytes но для удобства мы храним это значение отдельно так как оно часто нужно для отображения и анализа
    public long AvailableBytes { get; init;}




}