require 'albacore'

source = "Source/"
libraries = "Libraries/"
tools = "Tools/"

task :default => [ :build ]

desc "Build"
msbuild :build => [ :update_nuget ] do |msb|
  msb.properties = { :configuration => :Release }
  msb.verbosity = :Minimal
  msb.targets = [ :Clean, :Build ]
  msb.solution = "#{source}HotGlue.sln"
end

desc "Install Nuget Packages"
task :update_nuget do
  FileList["#{source}**/packages.config"].each { |filepath|
    sh "#{tools}NuGet.exe i #{filepath} -o #{libraries}"
  }
end

desc "Test"
nunit :test => :build do |nunit|
  nunit.command = "#{tools}NUnit/nunit-console.exe"
  nunit.assemblies "#{source}HotGlue.Tests/bin/release/HotGlue.Tests.dll"
end
