require 'albacore'

# Variables used throughout
source = "Source/"
libraries = "Libraries/"
tools = "Tools/"
deploy = "Deploy/"
version = "0.0.0"
version_changeset = ""

task :default => [ :test ]

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
  puts ""
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
nunit :test => [ :build ] do |nunit|
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

def ExecuteTask(task, *args)
  # So yeah this doesn't really support more than 1 argument
  Rake::Task[task].invoke(args[0])
  # Running .execute didn't seem to work property with albacore's arguments
  Rake::Task[task].reenable
end

task :nuget => [ :build ] do
  puts ""
  FileUtils.rm_rf("#{deploy}") if Dir.exists?("#{deploy}")
  Dir.mkdir("#{deploy}") unless Dir.exists?("#{deploy}")

	authors = "Nathan Palmer, Aaron Hansen"

	projects = [
		Project.new("#{source}HotGlue.Core/HotGlue.Core.csproj",version,"HotGlue.Core",authors, "HotGlue.Core"),
    Project.new("#{source}HotGlue.Aspnet/HotGlue.Aspnet.csproj",version,"HotGlue.Aspnet",authors, "HotGlue.Aspnet"),
    Project.new("#{source}HotGlue.Nancy/HotGlue.Nancy.csproj",version,"HotGlue.Nancy",authors, "HotGlue.Nancy"),
		Project.new("#{source}HotGlue.Compiler.CoffeeScript/HotGlue.Compiler.CoffeeScript.csproj",version,"HotGlue.Compiler.CoffeeScript",authors),
    Project.new("#{source}HotGlue.Compiler.TypeScript/HotGlue.Compiler.TypeScript.csproj",version,"HotGlue.Compiler.TypeScript",authors),
    Project.new("#{source}HotGlue.Compressor.Uglify/HotGlue.Compressor.Uglify.csproj",version,"HotGlue.Compressor.Uglify",authors),
    Project.new("#{source}HotGlue.Compressor.YUI/HotGlue.Compressor.YUI.csproj",version,"HotGlue.Compressor.YUI",authors),
    Project.new("#{source}HotGlue.Reference.LABjs/HotGlue.Reference.LABjs.csproj",version,"HotGlue.Reference.LABjs",authors),
    Project.new("#{source}HotGlue.Template.jQuery/HotGlue.Template.jQuery.csproj",version,"HotGlue.Template.jQuery",authors),
    Project.new("#{source}HotGlue.Template.Mustache/HotGlue.Template.Mustache.csproj",version,"HotGlue.Template.Mustache",authors),
    Project.new("#{source}HotGlue.Template.Handlebars/HotGlue.Template.Handlebars.csproj",version,"HotGlue.Template.Handlebars",authors),
    Project.new("#{source}HotGlue.Template.Dust/HotGlue.Template.Dust.csproj",version,"HotGlue.Template.Dust",authors),
    Project.new("#{source}HotGlue.Template.Underscore/HotGlue.Template.Underscore.csproj",version,"HotGlue.Template.Underscore",authors),
    Project.new("#{source}HotGlue.Template.EJS/HotGlue.Template.EJS.csproj",version,"HotGlue.Template.EJS",authors),
    Project.new("#{source}HotGlue.Template.NANO/HotGlue.Template.NANO.csproj",version,"HotGlue.Template.NANO",authors),
    Project.new("#{source}HotGlue.Template.JsRender/HotGlue.Template.JsRender.csproj",version,"HotGlue.Template.JsRender",authors),
    Project.new("#{source}HotGlue.Template.doT/HotGlue.Template.doT.csproj",version,"HotGlue.Template.doT",authors)
	]

	projects.each do |project|
    ExecuteTask(:nuspec, project)
    ExecuteTask(:nupack, project)
    File.delete "#{deploy}#{project.Name}.#{project.Version}.nuspec"
	end
end

nuspec :nuspec, [ :project ] do |nuspec, args|
  project = args.project
  puts "Generating nuspec for #{project.Name}"

  nuspec.id = project.Name
  nuspec.description = project.Description
  nuspec.version = project.Version
  nuspec.authors = project.Author
  nuspec.owners = project.Author
  nuspec.language = "en-US"
  project.Dependencies.each do |dep|
      nuspec.dependency dep.Name, dep.Version
  end
  nuspec.licenseUrl = "http://opensource.org/licenses/MIT"
  nuspec.working_directory = "#{deploy}"
  nuspec.output_file = "#{project.Name}.#{project.Version}.nuspec"
  nuspec.tags = ""
  nuspec.file "../#{File::dirname project.FilePath}/bin/Release/#{project.Name}.dll".gsub("/","\\"), "lib\\net40"
  if !Dir.glob("#{File::dirname project.FilePath}/*.transform").empty?
    nuspec.file "../#{File::dirname project.FilePath}/*.transform".gsub("/","\\"), "content"
  end
end

nugetpack :nupack, [ :project ] do |nuget, args|
  project = args.project
  nuget.command     = "#{tools}/Nuget.exe"
  nuget.nuspec      = "#{deploy}#{project.Name}.#{project.Version}.nuspec"
  nuget.output      = "#{deploy}"
end


