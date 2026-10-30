# GitHub Copilot Instructions for Temperature Gauge (Continued) Mod

## Mod Overview and Purpose
The Temperature Gauge (Continued) mod is an update of the Aeryl mod. It adds a simple buildable thermometer that displays the current ambient temperature and provides temperature-based alerts. The mod includes a wall-mounted gauge and updates to be compatible with the Parallel Power-grid mod. It also includes German and Russian translations.

## Key Features and Systems
- Wall-mounted gauge
- Temperature alerts for rooms
- Russian and German translations

## Coding Patterns and Conventions
- C# files are named using PascalCase, with the .cs extension.
- XML files are named using PascalCase, with the .xml extension.
- Types are named using PascalCase.
- Members are named using camelCase.
- Constants are named using all uppercase letters.
- XML tags are named using PascalCase.
- XML attributes are named using camelCase.

## XML Integration
The mod includes two XML files: Buildings_Misc.xml and TranslationTemplate.xml. The Buildings_Misc.xml file defines the thermometer building and its properties. The TranslationTemplate.xml file contains the translations for the mod's UI elements.

## Harmony Patching
The mod uses Harmony patching to extend the functionality of the game. Specifically, it uses Harmony to add a new building type and to modify the behavior of existing buildings.

## Suggestions for Copilot
- Use PascalCase for naming types and XML tags.
- Use camelCase for naming members and XML attributes.
- Use all uppercase letters for constants.
- Use Harmony patching to extend the functionality of the game.
- Use XML integration to define the mod's properties and behavior.

## Example Code Snippets

### C#
csharp
public class Building_TemperatureGauge : Building
{
    public float TargetTemperature;
    
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        TargetTemperature = 20f;
    }
    
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        
        yield return new Command_TemperatureGauge
        {
            TargetTemperature = TargetTemperature
        };
    }
}


### XML
xml
<ThingDef>
    <defName>TemperatureGauge</defName>
    <label>Temperature Gauge</label>
    <description>A simple thermometer that displays the current ambient temperature.</description>
    <category>Building</category>
    <comps>
        <li Class="Building_CompProperties_TemperatureGauge">
            <targetTemperature>20</targetTemperature>
        </li>
    </comps>
</ThingDef>

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

