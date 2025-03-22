# font.ca
```
"font"
{int:largura}
{int:altura}
{uint[largura * altura]}
{float:espaçamento_x}
{float:espaçamento_y}
{int:numero_de_glifos}
{
    (
        {int:caractere}
        {int:x}
        {int:y}
        {int:largura}
        {int:altura}
        {int:numero_de_pares}
        {
            (
                {int:caractere}
                {float:deslocamento}
            )
        }
    )[]
}
```
