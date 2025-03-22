# Fontes
Todos os assets serão compilados para `.build/[caminho_relativo]/[nome_do_arquivo].ca`.
As extensões suportadas são: `xml`.

## Formato do XML
```xml
<?xml version="1.0" encoding="utf-8"?>
<ClawFont>
    <Path>font.otf</Path><!-- Caminho da fonte, relativo ao arquivo (ttf, otf, woff ou woff2) -->
    <Size>100</Size><!-- Tamanho em que a fonte será compilada -->
    <Spacing X="0" Y="0" /><!-- Espaço entre os caracteres -->
    <UseKerning>true</UseKerning><!-- Se a fonte será compilada levando o kerning em consideração -->
    <Antialias>false</Antialias><!-- Se a fonte será compilada utilizando antialiasing -->
    <Style>Regular</Style><!-- Regular, Italic, Bold ou BoldItalic (maiúsculas/minúsculas não importam) -->
    <CharacterRegions><!-- Quais caracteres serão utilizados -->
        <Region Start="32" End="252" />
    </CharacterRegions>
</ClawFont>
```
