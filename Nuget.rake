# Nuget Generation
class Project
  attr_accessor :Name, :Description, :Author, :Version, :FilePath, :Platforms, :Dependencies
 
  def initialize(path, version, description, author, name = nil, dependencies = nil)
    @FilePath = path
    @Version = version
    @Name = name.nil? ? @FilePath.split(/\//).last.split(/.csproj/).first : name
    @Description = description
    @Author = author
    @Platforms = ["NET40"]
	if not dependencies.nil?
		@Dependencies = dependencies
	else
		@Dependencies = Array.new
	end
 
    LoadPackageDependencies()
    LoadProjectDependencies()
  end
 
  def LoadPackageDependencies()
    path = "#{File::dirname @FilePath}/packages.config"
 
    if File.exists? path and File.exists? "#{File::dirname @FilePath}/packages.config"
        packageConfigXml = File.read("#{File::dirname @FilePath}/packages.config")
        doc = REXML::Document.new(packageConfigXml)
        doc.elements.each("packages/package") do |package|
            @Dependencies << Dependency.new(package.attributes["id"], package.attributes["version"])
        end
    end
  end
 
  def LoadProjectDependencies()
    if @FilePath.match("csproj$") and File.exists? @FilePath
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