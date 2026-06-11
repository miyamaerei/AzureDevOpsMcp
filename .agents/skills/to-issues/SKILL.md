---
name: to-issues
description: "Break a plan, spec, or PRD into independently-grabbable Azure DevOps Work Items using tracer-bullet vertical slices. Use when: convert a plan into work items, create Azure Boards issues/tasks, create implementation tickets, or break down work into issues."
---

# To Issues

Break a plan into independently-grabbable Azure DevOps Work Items using vertical slices (tracer bullets).

Default tracker for this repository is Azure DevOps Boards. Prefer creating Work Items in the configured Azure DevOps project over GitHub issues.

## Azure DevOps defaults

If project configuration is available, use it to publish Work Items:

- Organization: `azureDefault.Org`
- Project: `azureDefault.Project`
- Repository: `azureDefault.Repo`
- Branch: `azureDefault.Branch`
- Assignee: `azureDefault.UserEmail`
- Work Item type: use `Task` unless the project exposes a more appropriate `Issue` or `User Story` type

Never print tokens or secrets. If a PAT is present in config, use it only for API calls.

## Work Item writing standards

Professional Azure DevOps Work Items should be queryable, actionable, and independently verifiable:

- Use a short imperative title that names the user-visible outcome.
- Put full detail in `System.Description` as HTML-compatible content.
- Include the sections: Parent, What to build, End-to-end behavior, Acceptance criteria, Blocked by, Priority, Source.
- Use numbered end-to-end behavior to show the tracer-bullet path.
- Use checkbox-style acceptance criteria in prose (`- [ ]` in Markdown drafts; `<ul><li>` in Azure HTML descriptions).
- Use dependency Work Item IDs in `Blocked by` after blockers are created.
- Add tags for discovery, for example `AzureWorkItemMCP; PRD; WorkItem; generated-by-copilot`.
- Set `Microsoft.VSTS.Common.Priority`: `1` for High, `2` for Medium, `3` for Low.

## Process

### 1. Gather context

Work from whatever is already in the conversation context. If the user passes a Work Item reference, URL, issue number, or local issue file path, fetch/read its full body and comments when tools are available.

### 2. Explore the codebase (optional)

If you have not already explored the codebase, do so to understand the current state of the code. Issue titles and descriptions should use the project's domain glossary vocabulary, and respect ADRs in the area you're touching.

### 3. Draft vertical slices

Break the plan into **tracer bullet** Work Items. Each Work Item is a thin vertical slice that cuts through ALL integration layers end-to-end, NOT a horizontal slice of one layer.

Slices may be 'HITL' or 'AFK'. HITL slices require human interaction, such as an architectural decision or a design review. AFK slices can be implemented and merged without human interaction. Prefer AFK over HITL where possible.

<vertical-slice-rules>
- Each slice delivers a narrow but COMPLETE path through every layer (schema, API, UI, tests)
- A completed slice is demoable or verifiable on its own
- Prefer many thin slices over few thick ones
</vertical-slice-rules>

### 4. Quiz the user

Present the proposed breakdown as a numbered list. For each slice, show:

- **Title**: short descriptive name
- **Type**: HITL / AFK
- **Blocked by**: which other slices (if any) must complete first
- **User stories covered**: which user stories this addresses (if the source material has them)
- **Suggested Work Item type**: Task / Issue / User Story
- **Priority**: High / Medium / Low

Ask the user:

- Does the granularity feel right? (too coarse / too fine)
- Are the dependency relationships correct?
- Should any slices be merged or split further?
- Are the correct slices marked as HITL and AFK?
- Should these be published to Azure DevOps Boards now?

Iterate until the user approves the breakdown.

### 5. Publish Work Items

For each approved slice, publish a new Azure DevOps Work Item in dependency order (blockers first) so later Work Items can reference real IDs in the `Blocked by` section.

Use the Azure DevOps REST API shape:

- `POST https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/${type}?api-version=7.1`
- Header: `Content-Type: application/json-patch+json`
- Body operations commonly include:
  - `/fields/System.Title`
  - `/fields/System.Description`
  - `/fields/System.AssignedTo`
  - `/fields/Microsoft.VSTS.Common.Priority`
  - `/fields/System.Tags`

When unsure about fields or Work Item type, first call `validateOnly=true` to verify the payload without saving.

<workitem-template>
## Parent

A reference to the parent Work Item on Azure DevOps Boards (if the source was an existing Work Item, otherwise `N/A - <reason>`).

## What to build

A concise description of this vertical slice. Describe the end-to-end behavior, not layer-by-layer implementation.

Avoid specific file paths or code snippets — they go stale fast. Exception: if a prototype produced a snippet that encodes a decision more precisely than prose can (state machine, reducer, schema, type shape), inline it here and note briefly that it came from a prototype. Trim to the decision-rich parts — not a working demo, just the important bits.

## End-to-end behavior

1. Trigger/action
2. System behavior
3. Verifiable outcome

## Acceptance criteria

- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

## Blocked by

- A reference to the blocking Work Item ID if any

Or `None - can start immediately` if no blockers.

## Priority

High / Medium / Low

## Source

PRD user stories, parent Work Item, or conversation source.
</workitem-template>

Do NOT close or modify any parent Work Item unless explicitly instructed.
