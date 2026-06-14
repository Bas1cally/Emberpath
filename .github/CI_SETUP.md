# CI setup — Unity Personal license

The CI (`/.github/workflows/ci.yml`) compiles the project and runs the EditMode
tests on every push using [game-ci](https://game.ci). It needs a Unity license.
For **Unity Personal** this is a one-time activation:

## 1. Get the activation file (.alf)

1. In GitHub, open the **Actions** tab.
2. Select **"Acquire Unity activation file"** → **Run workflow**.
3. When it finishes, open the run and download the artifact
   **`Manual Activation File`** — it contains a `Unity_v____.alf` file. Unzip it.

## 2. Convert it to a license file (.ulf)

1. Go to <https://license.unity3d.com/manual>.
2. Upload the `.alf` file.
3. Choose **Unity Personal Edition** (and "I don't use Unity in a professional
   capacity" if asked), then **Download** the resulting `Unity_v____.ulf`.

## 3. Add the repository secrets

In **Settings ▸ Secrets and variables ▸ Actions ▸ New repository secret**, add:

| Secret name      | Value                                                        |
|------------------|-------------------------------------------------------------|
| `UNITY_LICENSE`  | The **full contents** of the `.ulf` file (open it in a text editor, copy everything). |
| `UNITY_EMAIL`    | Your Unity account email.                                    |
| `UNITY_PASSWORD` | Your Unity account password.                                 |

## 4. Run CI

Push any commit (or re-run the **CI** workflow). The job will activate the
license, compile the project and run the EditMode smoke test. A green check means
all scripts compiled cleanly.

---

### Notes

- The **activation** workflow only needs to be run once. Keep the `.ulf` safe; a
  Personal license can be re-activated if needed.
- The CI runs on Linux and covers **compile + EditMode tests**. iOS builds require
  a macOS runner and Apple signing, which we'll add in a later step.
- Until the secrets are set, the CI job will fail at the license step — that is
  expected and harmless.
