import DOMPurify from "dompurify";
import { marked } from "marked";

marked.setOptions({ gfm: true, breaks: false });

export function renderAnswer(markdown: string): string {
  const html = marked.parse(markdown.replace(/\u202f/g, " "), { async: false }) as string;
  const safe = DOMPurify.sanitize(html, { USE_PROFILES: { html: true } });
  return safe
    .replaceAll("<table>", '<div class="table-wrap"><table>')
    .replaceAll("</table>", "</table></div>");
}
