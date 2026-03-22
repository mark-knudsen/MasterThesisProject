; ModuleID = 'repl_module'
source_filename = "repl_module"

@true_str = private unnamed_addr constant [6 x i8] c"True\0A\00", align 1
@false_str = private unnamed_addr constant [7 x i8] c"False\0A\00", align 1
@true_str.1 = private unnamed_addr constant [6 x i8] c"True\0A\00", align 1
@false_str.2 = private unnamed_addr constant [7 x i8] c"False\0A\00", align 1

define ptr @main_0() {
entry:
  br i1 true, label %then, label %else

then:                                             ; preds = %entry
  %print_bool = call i32 (ptr, ...) @printf(ptr @true_str)
  br label %ifcont

else:                                             ; preds = %entry
  %print_bool1 = call i32 (ptr, ...) @printf(ptr @false_str.2)
  br label %ifcont

ifcont:                                           ; preds = %else, %then
  %iftmp = phi i32 [ %print_bool, %then ], [ %print_bool1, %else ]
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
