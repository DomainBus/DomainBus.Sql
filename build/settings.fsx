#r "tools/FAKE/tools/FakeLib.dll"
open Fake

let projName="DomainBus.Sql"
let projDir= "..\src" @@ projName
let testDir="..\src" @@ "Tests"

let additionalPack=[|".SqlServer";".Sqlite"|]





