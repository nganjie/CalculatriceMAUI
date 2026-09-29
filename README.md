# Calculatrice .NET MAUI

Application de calculatrice mobile réalisée avec .NET MAUI (Activité n°4 – Atelier de développement mobile).

## Fonctionnalités

### Opérations de base
- Addition, soustraction, multiplication et division
- Saisie des nombres décimaux (touche `,`)
- Remise à zéro totale (`AC`) et effacement du dernier caractère (`⌫`)
- Changement de signe (`±`) et pourcentage (`%`)
  - `200 + 10 %` donne `200 + 20` ; seul, `50 %` donne `0,5`
- Division par zéro gérée sans plantage : le message « Division par zéro impossible » s'affiche et la touche suivante réinitialise la calculatrice
- Affichage de l'opération en cours au-dessus du résultat

### Opérations avancées
- Racine carrée (`√`), carré (`x²`) et inverse (`1/x`)
- Mémoire : `MC`, `MR`, `M+`, `M−` (un indicateur **M** apparaît quand la mémoire n'est pas vide)
- Enchaînement des opérations (`2 + 3 × 4` calcule au fil de la saisie) et répétition de la dernière opération par appuis successifs sur `=`
- Calculs en `decimal` (pas d'erreur du type `0,1 + 0,2 = 0,30000000000000004`)
- Gestion du dépassement de capacité et des entrées invalides (racine d'un nombre négatif)

### Interface
- Adaptation à l'orientation : écran au-dessus du clavier en portrait, écran à gauche et clavier à droite en paysage
- Taille des touches et du résultat proportionnelle à l'écran : rien ne déborde sur un petit écran, les longs nombres réduisent la police
- Thème clair et thème sombre

## Layouts utilisés

| Layout | Justification |
|---|---|
| `Grid` | Grille racine qui répartit l'écran entre l'affichage et le clavier (réorganisée en paysage), et grille 4 × 5 des touches pour des boutons de taille égale. |
| `Border` | Encadre la zone d'affichage avec des coins arrondis pour la distinguer du clavier, et sert de badge pour l'indicateur mémoire. |
| `VerticalStackLayout` | Empile la ligne de l'opération en cours au-dessus du résultat, alignés en bas de l'écran. |
| `HorizontalStackLayout` | Aligne côte à côte le titre et l'indicateur de mémoire en haut de l'affichage. |
| `ScrollView` | Permet de faire défiler horizontalement l'opération en cours lorsqu'elle est plus longue que l'écran. |
| `FlexLayout` | Répartit équitablement la largeur disponible entre les sept touches avancées, quelle que soit la taille de l'écran. |

## Structure

- `CalculatriceMAUI/CalculatorEngine.cs` : logique de calcul, indépendante de l'interface
- `CalculatriceMAUI/MainPage.xaml` : interface
- `CalculatriceMAUI/MainPage.xaml.cs` : gestionnaires d'événements et adaptation de la disposition

## Lancer le projet

```bash
dotnet build -t:Run -f net10.0-android
```
