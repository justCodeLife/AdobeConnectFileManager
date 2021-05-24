<template>
  <div class="error-container">
    <h1>404</h1>
    <h2>صفحه مورد نظر یافت نشد</h2>
    <p class="return">بازگشت به <a href="/">صفحه اصلی</a></p>
  </div>
</template>

<style scoped>
  .error-container {
    color: #e7e7e7;
    height: 100vh;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-content: center;
    text-align: center;
    background-color: #212121;
  }

  .error-container h1 {
    margin: 0;
    font-size: 130px;
    font-weight: 300;
  }

  .error-container h2 {
    color: #26A69A;
  }

  @media (min-width: 768px) {
    .error-container h1 {
      font-size: 220px;
    }
  }

  .return {
    position: absolute;
    width: 100%;
    bottom: 30px;
    font-weight: 400;
    letter-spacing: -0.04em;
    margin: 0;
  }

  .return a {
    padding-bottom: 1px;
    color: #26A69A;
    text-decoration: none;
  }

  .return a:hover {
    border-bottom-color: #fff;
  }
</style>
