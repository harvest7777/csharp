# cURL Examples

## POST /Article

```sh
curl -X POST http://localhost:5219/Article \
  -H "Content-Type: application/json" \
  -d '{"title": "My First Article", "content": "Hello world.", "slug": "my-first-article"}'
```
