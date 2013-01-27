require 'albacore'

# Variables used throughout
source = "Source/"
libraries = "Libraries/"
tools = "Tools/"
deploy = "Deploy/"
version = "0.0.0"
version_changeset = ""

task :default => [ :build ]

desc "Build"
msbuild :build => [ :update_nuget, :assembly_info ] do |msb|
  puts ""
  msb.properties = { :configuration => :Release }
  msb.verbosity = :Minimal
  msb.targets = [ :Clean, :Build ]
  msb.solution = "#{source}HotGlue.sln"
end

desc "Determine version"
task :version do
  result = %x[git describe]
  regex = /^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)(-(?<revision>\d+))*(-(?<changeset>\S*))*$/
  matches = regex.match(result)

  version = "#{matches[:major]}.#{matches[:minor]}.#{matches[:build]}.#{matches[:revision]}"
  version_changeset = "#{matches[:major]}.#{matches[:minor]}.#{matches[:build]}.#{matches[:changeset]}"
end

desc "Generate the AssemblyInfo"
assemblyinfo :assembly_info => [ :version ] do |asm|
  asm.version = version
  asm.file_version = version
  # Unsupported until the next release
  # https://github.com/Albacore/albacore/pull/30
  #asm.assembly_informational_version = version_changeset
  asm.copyright = "Copyright (c) 2012"
  asm.output_file = "#{source}/CommonAssemblyInfo.cs"
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

# Nuget Generation
class Project
  attr_accessor :Name, :Description, :Author, :Version, :FilePath, :Platforms, :Dependencies
 
  def initialize(path, version, description, author, name = nil)
    @FilePath = path
    @Version = version
    @Name = name.nil? ? @FilePath.split(/\//).last.split(/.csproj/).first : name
    @Description = description
    @Author = author
    @Platforms = ["NET40"]
    @Dependencies = Array.new
 
    LoadPackageDependencies()
    LoadProjectDependencies()
  end
 
  def LoadPackageDependencies()
    path = "#{File::dirname @FilePath}/packages.config"
 
    if File.exists? path
        packageConfigXml = File.read("#{File::dirname @FilePath}/packages.config")
        doc = REXML::Document.new(packageConfigXml)
        doc.elements.each("packages/package") do |package|
            @Dependencies << Dependency.new(package.attributes["id"], package.attributes["version"])
        end
    end
  end
 
  def LoadProjectDependencies()
    if File.exists? @FilePath
        projectFileXml = File.read(@FilePath)
        doc = REXML::Document.new(projectFileXml)
        doc.elements.each("Project/ItemGroup/ProjectReference/Name") do |proj|
            @Dependencies << Dependency.new(proj.text, @Version)
        end
    end
  end
end

class Dependency
    attr_accessor :Name, :Version
 
    def initialize(name, version)
        @Name = name
        @Version = version
    end
end

task :nuspec do
  Dir.mkdir("#{deploy}") unless Dir.exists?("#{deploy}")

	version = "0.1"
	authors = "Nathan Palmer, Aaron Hansen"

	projects = [
		Project.new("#{source}HotGlue.Core/HotGlue.Core.csproj",version,"HotGlue",authors, "HotGlue"),
		Project.new("#{source}HotGlue.Compiler.CoffeeScript/HotGlue.Compiler.CoffeeScript.csproj",version,"HotGlue.Compiler.CoffeeScript",authors),
    Project.new("#{source}HotGlue.Compressor.Uglify/HotGlue.Compressor.Uglify.csproj",version,"HotGlue.Compressor.Uglify",authors),
    Project.new("#{source}HotGlue.Compressor.YUI/HotGlue.Compressor.YUI.csproj",version,"HotGlue.Compressor.YUI",authors),
    Project.new("#{source}HotGlue.Reference.LABjs/HotGlue.Reference.LABjs.csproj",version,"HotGlue.Reference.LABjs",authors),
    Project.new("#{source}HotGlue.Template.jQuery/HotGlue.Template.jQuery.csproj",version,"HotGlue.Template.jQuery",authors)
	]

	projects.each do |project|
		nuspec do |nuspec|
		  nuspec.id = project.Name
		  nuspec.description = project.Description
		  nuspec.version = project.Version
		  nuspec.authors = project.Author
		  nuspec.owners = project.Author
		  nuspec.language = "en-US"
		  project.Dependencies.each do |dep|
		      nuspec.dependency dep.Name, dep.Version
		  end
		  nuspec.licenseUrl = "http://www.gnu.org/licenses/lgpl.txt"
		  nuspec.working_directory = "#{deploy}"
		  nuspec.output_file = "#{project.Name}.#{project.Version}.nuspec"
		  nuspec.tags = ""
      nuspec.file "../#{File::dirname project.FilePath}/bin/Release/*.dll".gsub("/","\\"), "lib\\net40"
		end

    sh "#{tools}Nuget.exe pack #{deploy}#{project.Name}.#{project.Version}.nuspec -OutputDirectory #{deploy}"

    #File.delete "#{deploy}#{project.Name}.#{project.Version}.nuspec"
	end
end

