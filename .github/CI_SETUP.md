# CI setup — Unity Personal license

The CI (`/.github/workflows/ci.yml`) compiles the project and runs the EditMode
tests on every push using [game-ci](https://game.ci). It needs a Unity license.
For **Unity Personal** this is a one-time activation:

> **Have Unity installed locally? Skip the GitHub workflow.** You can create the
> activation file (`.alf`) directly with your local editor — see
> *"Alternative: create the activation file locally"* below — then jump to step 2.

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

## Alternative: create the activation file locally

If Unity is already installed, you don't need the GitHub activation workflow at
all. Run the editor once in batch mode to produce the `.alf`, then continue at
**step 2** above.

Find your editor path in **Unity Hub ▸ Installs** (gear icon ▸ *Show in
Explorer/Finder*), then:

**macOS**
```bash
cd ~/Desktop
"/Applications/Unity/Hub/Editor/<VERSION>/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -createManualActivationFile -logFile -
```

**Windows (PowerShell)**
```powershell
cd $HOME\Desktop
& "C:\Program Files\Unity\Hub\Editor\<VERSION>\Editor\Unity.exe" `
  -batchmode -nographics -createManualActivationFile -logFile -
```

Replace `<VERSION>` with your installed editor (e.g. `6000.0.32f1`). A file like
`Unity_v6000.0.x.alf` is written to the current folder (here: the Desktop). Any
installed Unity version works — the resulting Personal `.ulf` is not tied to a
specific version. Then continue with **step 2** (convert) and **step 3**
(secrets).

The CI runs on every branch, so once the secrets exist you don't need to merge
anything — just re-run the failed **CI** run or push a new commit.

---

### Notes

- The **activation** workflow only needs to be run once. Keep the `.ulf` safe; a
  Personal license can be re-activated if needed.
- The CI runs on Linux and covers **compile + EditMode tests**. iOS builds require
  a macOS runner and Apple signing, which we'll add in a later step.
- Until the secrets are set, the CI job will fail at the license step — that is
  expected and harmless.
