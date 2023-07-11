﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using CUE4Parse_Conversion.Textures;
using CUE4Parse.GameTypes.FN.Enums;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.GameplayTags;
using CUE4Parse.Utils;
using Saturn.Backend.Data.Variables;
using SkiaSharp;

namespace Saturn.Backend.Data.Swapper.Assets;

public class AssetSelectorItem
{
    public SKBitmap IconBitmap;
    public SKBitmap FullBitmap;
    public FGameplayTagContainer GameplayTags;

    public UObject Asset { get; set; }
    public EAssetType Type { get; set; }
    public SKData FullSource { get; set; }
    public string DisplayName { get; set; }
    public string DisplayNameSource { get; set; }
    public string Description { get; set; }
    public bool IsRandom { get; set; }
    public string TooltipName { get; set; }
    public string ID { get; set; }
    public EFortRarity Rarity { get; set; }
    public int SeasonNumber { get; set; }
    public string Series { get; set; }
    
    public bool HiddenAsset;

    public AssetSelectorItem(UObject asset, UTexture2D previewTexture, EAssetType type, bool isRandomSelector = false,
        FText? displayNameOverride = null, string? descriptionOverride = null, bool hiddenAsset = false)
    {
        Type = type;

        Asset = asset;
        var displayName = displayNameOverride;
        displayName ??= asset.GetOrDefault("DisplayName", new FText("Unnamed"));
        HiddenAsset = hiddenAsset;

        DisplayName = displayName.Text;
        if (DisplayName.Equals("TBD") || string.IsNullOrWhiteSpace(DisplayName))
        {
            DisplayName = asset.Name;
        }

        if (displayName.TextHistory is FTextHistory.Base textHistory)
        {
            DisplayNameSource = textHistory.SourceString;
        }
        ID = asset.Name;
        Description = descriptionOverride ?? asset.GetOrDefault("Description", new FText("No Description.")).Text;

        Rarity = asset.GetOrDefault("Rarity", EFortRarity.Uncommon);
        GameplayTags = asset.GetOrDefault<FGameplayTagContainer>("GameplayTags");

        var seasonTag = GameplayTags.GetValueOrDefault("Cosmetics.Filter.Season.")?.Text.SubstringAfterLast(".");
        SeasonNumber = int.TryParse(seasonTag, out var seasonNumber) ? seasonNumber : int.MaxValue;
        if (asset.TryGetValue<UObject>(out var series, "Series"))
        {
            Series = series.GetOrDefault<FText>("DisplayName").Text;
        }

        TooltipName = $"{DisplayName} ({ID})";
        IsRandom = isRandomSelector;

        var iconBitmap = previewTexture.Decode();
        if (iconBitmap is null) return;
        IconBitmap = iconBitmap;

        FullBitmap = new SKBitmap(iconBitmap.Width, iconBitmap.Height, iconBitmap.ColorType, iconBitmap.AlphaType);
        using (var fullCanvas = new SKCanvas(FullBitmap))
        {
            DrawBackground(fullCanvas, Math.Max(iconBitmap.Width, iconBitmap.Height));
            fullCanvas.DrawBitmap(iconBitmap, 0, 0);
        }

        FullSource = FullBitmap.Encode(SKEncodedImageFormat.Png, 100);
    }

    public string GetHTMLImage()
    {
        return "data:image/png;base64," + Convert.ToBase64String(FullSource.ToArray());
    }

    private const int MARGIN = 2;

    private void DrawBackground(SKCanvas canvas, int size)
    {
        SKShader BorderShader(params FLinearColor[] colors)
        {
            var parsedColors = colors.Select(x => SKColor.Parse(x.Hex)).ToArray();
            return SKShader.CreateLinearGradient(new SKPoint(size / 2f, size), new SKPoint(size, size / 4f), parsedColors, SKShaderTileMode.Clamp);
        }

        SKShader BackgroundShader(params FLinearColor[] colors)
        {
            var parsedColors = colors.Select(x => SKColor.Parse(x.Hex)).ToArray();
            return SKShader.CreateRadialGradient(new SKPoint(size / 2f, size / 2f), size / 5 * 4, parsedColors, SKShaderTileMode.Clamp);
        }

        if (Type == EAssetType.Prop)
        {
            canvas.DrawRect(new SKRect(0, 0, size, size), new SKPaint()
            {
                Color = SKColor.Parse("707370")
            });
            return;
        }

        if (Asset.TryGetValue(out UObject seriesData, "Series"))
        {
            var colors = seriesData.Get<RarityCollection>("Colors");
            
            canvas.DrawRect(new SKRect(0, 0, size, size), new SKPaint()
            {
                Shader = BorderShader(colors.Color2, colors.Color1)
            });

            if (seriesData.TryGetValue(out UTexture2D background, "BackgroundTexture"))
            {
                canvas.DrawBitmap(background.Decode(), new SKRect(MARGIN, MARGIN, size - MARGIN, size - MARGIN));
            }
            else
            {
                canvas.DrawRect(new SKRect(MARGIN, MARGIN, size - MARGIN, size - MARGIN), new SKPaint()
                {
                    Shader = BackgroundShader(colors.Color1, colors.Color3)
                });
            }
        }
        else
        {
            var colorData = Constants.RarityData[(int)Rarity];
            
            canvas.DrawRect(new SKRect(0, 0, size, size), new SKPaint()
            {
                Shader = BorderShader(colorData.Color2, colorData.Color1)
            });
            
            canvas.DrawRect(new SKRect(MARGIN, MARGIN, size - MARGIN, size - MARGIN), new SKPaint()
            {
                Shader = BackgroundShader(colorData.Color1, colorData.Color3)
            });
        }
    }
}