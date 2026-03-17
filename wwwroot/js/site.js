document.addEventListener("DOMContentLoaded", function () {
  // Auto dismiss alerts
  document.querySelectorAll(".alert-success, .alert-error").forEach(function (el) {
    setTimeout(function () {
      el.style.transition = "opacity 0.4s, transform 0.4s";
      el.style.opacity = "0";
      el.style.transform = "translateY(-6px)";
      setTimeout(function () { el.remove(); }, 400);
    }, 3500);
  });

  // Animate cat bars
  document.querySelectorAll(".cat-bar-fill").forEach(function (bar) {
    var width = bar.style.width;
    bar.style.width = "0";
    setTimeout(function () { bar.style.width = width; }, 150);
  });

  // Scroll active tab into view (mobile)
  var activeTab = document.querySelector(".tab.active");
  if (activeTab) {
    setTimeout(function() {
      activeTab.scrollIntoView({ behavior: "smooth", block: "nearest", inline: "center" });
    }, 100);
  }

  // Add touch feedback to cards
  document.querySelectorAll(".expense-card, .user-card, .summary-item").forEach(function(card) {
    card.addEventListener("touchstart", function() {
      this.style.opacity = "0.85";
    }, { passive: true });
    card.addEventListener("touchend", function() {
      this.style.opacity = "1";
    }, { passive: true });
  });
});
