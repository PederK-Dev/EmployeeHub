# EmployeeHub — frontend

The React single-page client for EmployeeHub. See the [root README](../README.md) for the full
project overview, feature list, and API documentation.

## Running locally

The client needs the API running as well — start it first (see the root README), then:

```bash
cp .env.example .env   # first time only
npm install            # first time only
npm run dev
```

The app is served at <http://localhost:5173> and signs in against the API address configured in
`.env` (`VITE_API_URL`).

## Scripts

| Script            | Description                                       |
| ----------------- | ------------------------------------------------- |
| `npm run dev`     | Start the Vite dev server with hot reload         |
| `npm run build`   | Type-check (`tsc -b`) and build for production    |
| `npm run preview` | Serve the production build locally                |

## Layout

```text
src/
├── pages/        # Route components (login, dashboard, employees, ...)
├── components/   # App components + the Untitled UI component library
├── providers/    # Auth, theme, toast, and router context
├── lib/          # Typed API client and shared types
├── hooks/        # Custom hooks
├── styles/       # Global styles and theme tokens
└── utils/        # Helpers (cx, etc.)
```

## Built on Untitled UI

The UI is composed from [Untitled UI React](https://www.untitledui.com/react), scaffolded from the
official Vite starter kit. Its open-source components are MIT licensed. Icons come from the
`@untitledui/icons` package and are used under
[Untitled UI's license](https://www.untitledui.com/license) — they may be used in this project but
not extracted or redistributed as an icon library.
