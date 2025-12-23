// 设置背景图像的数组
const images = [
    '../images/1.webp',
    '../images/2.webp',
    '../images/3.webp',
    '../images/4.webp',
    '../images/5.webp',
    '../images/6.webp'
];

// 当前背景图像的索引
let currentImageIndex = 0;

// 设置背景图像
function changeBackground() {
    // 获取下一个背景图像
    document.body.style.backgroundImage = `url(${images[currentImageIndex]})`;

    // 更新索引，确保循环切换
    currentImageIndex = (currentImageIndex + 1) % images.length;
}

// 每5秒切换一次背景图像
setInterval(changeBackground, 5000);

// 初始设置背景图像
changeBackground();