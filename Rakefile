require 'albacore'

source = "Source/"
libraries = "Libraries/"
tools = "Tools/"

task :default => [ :build ]

desc "Build"
msbuild :build => [ :update_nuget ] do |msb|
  puts ""
  msb.properties = { :configuration => :Release }
  msb.verbosity = :Minimal
  msb.targets = [ :Clean, :Build ]
  msb.solution = "#{source}HotGlue.sln"
end

desc "Install Nuget Packages"
task :update_nuget do
  puts ""
  FileList["#{source}**/packages.config"].each { |filepath|
  	verbose(false) do
  		puts "Installing packages for #{filepath} "
    	sh "#{tools}NuGet.exe i #{filepath} -o #{libraries}"
    end
  }
end

desc "Test"
nunit :test => :build do |nunit|
  puts ""
  nunit.command = "#{tools}NUnit/nunit-console.exe"
  nunit.assemblies = FileList["#{source}*/bin/Release/*.Tests.dll"]
end