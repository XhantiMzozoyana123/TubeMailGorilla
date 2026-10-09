        var w = (int)(width / 8) * 8;
        var h = (int)(height / 8) * 8;

        var graph = new Dictionary<string, object?>
        {
            ["1"] = new Dictionary<string, object?>
            {
                ["class_type"] = "CheckpointLoaderSimple",
                ["inputs"] = new Dictionary<string, object?> { ["ckpt_name"] = checkpoint },
            },
            ["2"] = new Dictionary<string, object?>
            {
                ["class_type"] = "CLIPTextEncode",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["text"] = $"cinematic still, {prompt}",
                    ["clip"] = new object?[] { "1", 1 },
                },
            },
            ["3"] = new Dictionary<string, object?>
            {
                ["class_type"] = "CLIPTextEncode",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["text"] = "blurry, watermark, text, logo, deformed, low quality",
                    ["clip"] = new object?[] { "1", 1 },
                },
            },
            ["4"] = new Dictionary<string, object?>
            {
                ["class_type"] = "LoadImage",
                ["inputs"] = new Dictionary<string, object?> { ["image"] = imageName },
            },
            ["9"] = new Dictionary<string, object?>
            {
                ["class_type"] = "ImageScale",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["image"] = new object?[] { "4", 0 },
                    ["width"] = w,
                    ["height"] = h,
                    ["upscale_method"] = "bilinear",
                    ["crop"] = "disabled",
                },
            },
            ["5"] = new Dictionary<string, object?>
            {
                ["class_type"] = "VAEEncode",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["pixels"] = new object?[] { "9", 0 },
                    ["vae"] = new object?[] { "1", 2 },
                },
            },
            ["6"] = new Dictionary<string, object?>
            {
                ["class_type"] = "KSampler",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["model"] = new object?[] { "1", 0 },
                    ["positive"] = new object?[] { "2", 0 },
                    ["negative"] = new object?[] { "3", 0 },
                    ["latent_image"] = new object?[] { "5", 0 },
                    ["seed"] = seed,
                    ["steps"] = steps,
                    ["cfg"] = 8.0,
                    ["sampler_name"] = "euler_ancestral",
                    ["scheduler"] = "normal",
                    ["denoise"] = denoise,
                },
            },
            ["7"] = new Dictionary<string, object?>
            {
                ["class_type"] = "VAEDecode",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["samples"] = new object?[] { "6", 0 },
                    ["vae"] = new object?[] { "1", 2 },
                },
            },
            ["8"] = new Dictionary<string, object?>
            {
                ["class_type"] = "SaveImage",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["images"] = new object?[] { "7", 0 },
                    ["filename_prefix"] = "tmg_snapshot_ai",
                },
            },
        };

        var payload = JsonSerializer.Serialize(new { prompt = graph }, JsonOptions);
