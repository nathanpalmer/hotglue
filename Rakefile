require 'albacore'

source = "Source\\"

task :default => [ :build ]

desc "Build"
msbuild :build do |msb|
  msb.properties = { :configuration => :Release }
  msb.verbosity = :Minimal
  msb.targets = [ :Clean, :Build ]
  msb.solution = "#{source}HotGlue.sln"
end