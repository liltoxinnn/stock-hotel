# Gestion de stock

Application Windows de gestion d'inventaire, simple et rapide.
Interface entièrement en français, fonctionnement 100 % hors connexion.

## Fonctionnalités

- Liste des produits avec la quantité en stock
- Ajout au stock (`+`) et retrait du stock (`−`) en deux clics
- Création, modification et suppression de produits
- Recherche instantanée (insensible à la casse et aux accents)
- Historique des entrées et sorties, du plus récent au plus ancien
- Indicateurs : nombre de produits, total des articles, produits en stock faible
- Seuil de stock faible configurable (2 par défaut)
- Enregistrement automatique : chaque modification est écrite immédiatement

## Technologies

| Élément        | Choix                            |
| -------------- | -------------------------------- |
| Langage        | C#                               |
| Framework      | .NET 8 (`net8.0-windows`)        |
| Interface      | WPF, architecture MVVM           |
| Base de données| SQLite via Entity Framework Core |

Aucun serveur, aucun navigateur et aucune base de données externe ne sont nécessaires.

## Prérequis

- Windows 10 ou Windows 11
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) ou Visual Studio 2022
  (charge de travail « Développement .NET Desktop »)

## Lancer l'application

Avec Visual Studio : ouvrir `StockManager.sln`, puis démarrer le projet (F5).

En ligne de commande :

```bash
dotnet run --project StockManager
```

Au premier démarrage, la base de données est créée automatiquement avec
l'inventaire de départ (8 produits, 46 articles au total). Les lancements
suivants réutilisent l'inventaire réel de l'utilisateur.

## Emplacement des données

```
%APPDATA%\StockManager\stock.db
```

Ce chemin est également affiché dans la page **Paramètres**. Sauvegarder ce
fichier suffit à sauvegarder tout l'inventaire.

## Inventaire de départ

| Produit            | Stock |
| ------------------ | ----: |
| Housse de couettes |     9 |
| Drap housse        |     9 |
| Grandes serviettes |     9 |
| Petites serviettes |     6 |
| Taies              |     9 |
| Tapis              |     1 |
| Serviettes mains   |     1 |
| Torchons           |     2 |

## Structure du projet

```
StockManager/
├── Models/            Product, StockMovement, MovementType, AppSetting
├── Data/              AppDbContext (EF Core / SQLite), AppPaths
├── Services/          StockService (règles métier), DialogService, validation
├── ViewModels/        MVVM : pages et fenêtres
├── Views/             MainWindow, pages Stock / Historique / Paramètres, fenêtres
├── Converters/        Convertisseurs de liaison
├── Core/              ObservableObject, RelayCommand
└── Resources/Styles/  Palette, pictogrammes, styles des contrôles
```

La logique métier (`Services/StockService.cs`) ne dépend pas de l'interface :
elle contient toutes les règles et tous les accès à la base de données.

## Règles métier

1. Le stock ne peut jamais devenir négatif ; un retrait supérieur au stock
   disponible est refusé avec le message
   « Stock insuffisant. La quantité disponible est de N. »
2. Toute modification du stock est enregistrée dans l'historique
   (produit, type, quantité, date).
3. Les noms de produits sont obligatoires et uniques, sans tenir compte de la casse.
4. Les quantités d'un mouvement sont des nombres entiers strictement positifs.
5. La suppression d'un produit demande une confirmation et retire également
   son historique de mouvements.
6. Les données sont conservées à la fermeture de l'application.
