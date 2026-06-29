(function () {
  var CHANGELOG_URL =
    "https://raw.githubusercontent.com/zv8001/RBX-Audio-Extractor/refs/heads/main/CHANGELOG.md";

  function escapeHtml(str) {
    return str
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");
  }

  function inlineMarkdown(str) {
    var html = escapeHtml(str);
    html = html.replace(/`([^`]+)`/g, "<code>$1</code>");
    html = html.replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>");
    html = html.replace(/\[([^\]]+)\]\(([^)]+)\)/g, function (m, text, href) {
      return '<a href="' + href + '" target="_blank" rel="noopener">' + text + "</a>";
    });
    return html;
  }

  function parseLatestEntry(markdown) {
    var lines = markdown.split(/\r?\n/);
    var entry = null;
    var section = null;

    for (var i = 0; i < lines.length; i++) {
      var line = lines[i];
      var versionMatch = line.match(/^##\s+\[([^\]]+)\]\s*-\s*(.+)$/);

      if (versionMatch) {
        if (entry) break; // stop once the first (latest) entry is closed
        entry = { version: versionMatch[1], date: versionMatch[2].trim(), sections: [] };
        section = null;
        continue;
      }

      if (!entry) continue;

      var sectionMatch = line.match(/^###\s+(.+)$/);
      if (sectionMatch) {
        section = { title: sectionMatch[1].trim(), items: [] };
        entry.sections.push(section);
        continue;
      }

      var itemMatch = line.match(/^[-*]\s+(.+)$/);
      if (itemMatch && section) {
        section.items.push(itemMatch[1].trim());
      }
    }

    return entry;
  }

  function render(entry) {
    var heading = document.getElementById("changelog-heading");
    var content = document.getElementById("changelog-content");

    if (!entry) {
      heading.textContent = "Latest release";
      content.innerHTML = '<p class="changelog-error">Could not load the changelog right now.</p>';
      return;
    }

    heading.textContent = "Latest release — " + entry.version;

    var html = '<p class="changelog-date">' + escapeHtml(entry.date) + "</p>";
    entry.sections.forEach(function (section) {
      html += "<h3>" + escapeHtml(section.title) + "</h3><ul>";
      section.items.forEach(function (item) {
        html += "<li>" + inlineMarkdown(item) + "</li>";
      });
      html += "</ul>";
    });

    content.innerHTML = html;
  }

  function loadChangelog() {
    fetch(CHANGELOG_URL, { cache: "no-store" })
      .then(function (res) {
        if (!res.ok) throw new Error("Failed to fetch changelog: " + res.status);
        return res.text();
      })
      .then(function (markdown) {
        render(parseLatestEntry(markdown));
      })
      .catch(function () {
        render(null);
      });
  }

  document.addEventListener("DOMContentLoaded", loadChangelog);
})();
