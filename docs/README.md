# R7 Test Task

Практическое задание

## Описание

Создать словарь (`Dictionary`), который сможет хранить максимум 2 в 62 ключ-значений, в отличии от текущей реализации `Dictionary`

### Пример

```cs
void StartTestDict()
{
  var len = long.MaxValue >> 2;
  
  var sw = new Stopwatch();
  var dict = new Dictionary<long, long>(210);
  sw.Start();
  for (long i = 0; i < len; i++)
  {
    dict[i] = i;
    if (i % 1000000 == 0 && i != 0)
    {
      Console.WriteLine($"{i};{sw.Elapsed}");
      sw.Restart();
    }
  } 
}

void StartTestLargeDict()
{
  var len = long.MaxValue >> 2;
  
  var sw = new Stopwatch();
  var dict = new LargeDictionary<long, long>();
  sw.Start();
  for (long i = 0; i < len; i++)
  {
    dict[i] = i;
    if (i % 1000000 == 0 && i != 0)
    {
      Console.WriteLine($"{i};{sw.Elapsed}");
      sw.Restart();
    }
  } 
}
```