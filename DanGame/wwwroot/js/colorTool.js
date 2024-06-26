function getMainColors(imageUrl, numColors) {
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    const img = new Image();
    img.crossOrigin = "Anonymous";
    img.src = imageUrl;
    return new Promise((resolve, ) => {
        img.onload = function () {
            try {
                canvas.width = img.width;
                canvas.height = img.height;
                ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
                const colorThief = new ColorThief();
                if (numColors == 1) {
                    resolve(colorThief.getColor(img));
                } else {
                    resolve(colorThief.getPalette(img, numColors));
                }
            } catch (error) {
                reject(error);
            }
        };
    });
}


function drawElmBackgroundColorByImage(elm, imageUrl) {
    getMainColors(imageUrl, numColors).then((result) => {
        const mainColors = result;
        elm.style['background'] = 'rgb(' + mainColors[0] + ',' + mainColors[1] + ',' + mainColors[2] + ')';
    })
}

function drawElmBackgroundGradientByImage(elm, imageUrl, numColors = 2) {
    elm = $(elm);
    getMainColors(imageUrl, numColors).then((result) => {
        const mainColors = result;

        if (numColors == 1) {
            const gradientColor = "rgb(" + mainColors.join(', ') + ")";
            elm.css("background", `linear-gradient(to top, ${gradientColor}, ${gradientColor})`);
        } else {
            const gradientColors = mainColors.map(color => `rgb(${color.join(',')})`).join(', ');
            elm.css("background", `linear-gradient(to top, ${gradientColors})`);
        }
    });
}