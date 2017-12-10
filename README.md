# Santafy

This repository contains the code presented in the blog post, [The soccer player best suited to be Santa Claus](https://martinand.net/2017/12/11/the-soccer-player-best-suited-to-be-santa-claus). The blog post is part of the [F# Advent Calendar in English 2017](https://sergeytihon.com/2017/10/22/f-advent-calendar-in-english-2017/) and, as such, I thought it would be fitting with a Christmas theme. We'll use [EA SPORTS FUT Database](https://www.easports.com/fifa/ultimate-team/fut/database), [Microsoft Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/face/) and [F#](http://fsharp.org/) to find the soccer player best suited to be Santa Claus 🎅

## Getting Started

The following assumes that F# and [Paket](https://github.com/fsprojects/Paket) are available.

1. Restore dependencies using Paket, e.g. `paket restore` or `Paket: Restore` in [Ionide](https://github.com/ionide).
2. [Get Face API Key](https://azure.microsoft.com/en-us/try/cognitive-services/?api=face-api) and replace `<insert key here>` in `Santafy.fsx`. 
3. Execute F# Script, `Santafy.fsx`.

The Face API has a rate limit that depends on your subscription, see [Pricing Details](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/face-api/). If you get _Access Denied_, you can fix it by throttling the requests by adding a delay.