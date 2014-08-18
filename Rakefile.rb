require 'rake'
require 'albacore'

$projectSolution = 'src/MongoMigrations.sln'
$artifactsPath = "build"
$nugetFeedPath = ENV["NuGetDevFeed"]
$srcPath = File.expand_path('src')

task :build => [:build_release]

msbuild :build_release => [:clean] do |msb|
  msb.properties :configuration => :Release
  msb.targets :Build
  msb.solution = $projectSolution
end

task :clean do
    puts "Cleaning"
    FileUtils.rm_rf $artifactsPath
	bins = FileList[File.join($srcPath, "**/bin")].map{|f| File.expand_path(f)}
	bins.each do |file|
		sh %Q{rmdir /S /Q "#{file}"}
    end
end

task :nuget => [:build] do
	sh "nuget pack src\\MongoMigrations\\MongoMigrations.csproj /OutputDirectory " + $nugetFeedPath
end