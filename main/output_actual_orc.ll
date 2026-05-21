; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_x = type { i64 }

@x = global ptr null, align 8

define ptr @main_1() {
entry:
  %record_ptr = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_x, ptr null, i32 1) to i64))
  %field_0 = getelementptr %struct_x, ptr %record_ptr, i32 0, i32 0
  store i64 5, ptr %field_0, align 8
  store ptr %record_ptr, ptr @x, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
