namespace Otor.MsixHero.Winget.Yaml.Entities
{
    // {
    //   "type": [ "object", "null" ],
    //   "properties": {
    //     "WindowsFeatures": {
    //       "type": [ "array", "null" ],
    //       "items": {
    //         "type": "string",
    //         "minLength": 1,
    //         "maxLength": 128
    //       },
    //       "maxItems": 16,
    //       "uniqueItems": true,
    //       "description": "List of Windows feature dependencies"
    //     },
    //     "WindowsLibraries": {
    //       "type": [ "array", "null" ],
    //       "items": {
    //         "type": "string",
    //         "minLength": 1,
    //         "maxLength": 128
    //       },
    //       "maxItems": 16,
    //       "uniqueItems": true,
    //       "description": "List of Windows library dependencies"
    //     },
    //     "PackageDependencies": {
    //       "type": [ "array", "null" ],
    //       "items": {
    //         "type": "object",
    //         "properties": {
    //         "PackageIdentifier": {
    //           "$ref": "#/definitions/PackageIdentifier"
    //         },
    //         "MinimumVersion": {
    //           "$ref": "#/definitions/PackageVersion"
    //         }
    //       },
    //       "required": [ "PackageIdentifier" ]
    //     },
    //     "maxItems": 16,
    //     "description": "List of package dependencies from current source"
    //   },
    //   "ExternalDependencies": {
    //     "type": [ "array", "null" ],
    //     "items": {
    //       "type": "string",
    //         "minLength": 1,
    //         "maxLength": 128
    //       },
    //       "maxItems": 16,
    //       "uniqueItems": true,
    //       "description": "List of external package dependencies"
    //     }
    //   }
    // }
    public class YamlDependencies
    {
    }
}
