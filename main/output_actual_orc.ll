; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr
@str = private unnamed_addr constant [10 x i8] c"voldemort\00", align 1
@str.1 = private unnamed_addr constant [11 x i8] c"dumbledore\00", align 1
@str.2 = private unnamed_addr constant [7 x i8] c"MERLIN\00", align 1
@str.3 = private unnamed_addr constant [10 x i8] c"voldemort\00", align 1
@str.4 = private unnamed_addr constant [11 x i8] c"dumbledore\00", align 1
@str.5 = private unnamed_addr constant [7 x i8] c"MERLIN\00", align 1

define ptr @main_1() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 48)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 3, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 6, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %record_buffer = call ptr @malloc(i64 24)
  %field_mem = call ptr @malloc(i64 8)
  %cast = bitcast ptr %field_mem to ptr
  store i64 5, ptr %cast, align 4
  %field_ptr = getelementptr ptr, ptr %record_buffer, i64 0
  store ptr %field_mem, ptr %field_ptr, align 8
  %field_ptr1 = getelementptr ptr, ptr %record_buffer, i64 1
  store ptr @str, ptr %field_ptr1, align 8
  %field_mem2 = call ptr @malloc(i64 8)
  %cast3 = bitcast ptr %field_mem2 to ptr
  store i64 80, ptr %cast3, align 4
  %field_ptr4 = getelementptr ptr, ptr %record_buffer, i64 2
  store ptr %field_mem2, ptr %field_ptr4, align 8
  %val_to_ptr = bitcast ptr %record_buffer to ptr
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr %val_to_ptr, ptr %elem_ptr, align 8
  %record_buffer5 = call ptr @malloc(i64 24)
  %field_mem6 = call ptr @malloc(i64 8)
  %cast7 = bitcast ptr %field_mem6 to ptr
  store i64 6, ptr %cast7, align 4
  %field_ptr8 = getelementptr ptr, ptr %record_buffer5, i64 0
  store ptr %field_mem6, ptr %field_ptr8, align 8
  %field_ptr9 = getelementptr ptr, ptr %record_buffer5, i64 1
  store ptr @str.1, ptr %field_ptr9, align 8
  %field_mem10 = call ptr @malloc(i64 8)
  %cast11 = bitcast ptr %field_mem10 to ptr
  store i64 70, ptr %cast11, align 4
  %field_ptr12 = getelementptr ptr, ptr %record_buffer5, i64 2
  store ptr %field_mem10, ptr %field_ptr12, align 8
  %val_to_ptr13 = bitcast ptr %record_buffer5 to ptr
  %elem_ptr14 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr %val_to_ptr13, ptr %elem_ptr14, align 8
  %record_buffer15 = call ptr @malloc(i64 24)
  %field_mem16 = call ptr @malloc(i64 8)
  %cast17 = bitcast ptr %field_mem16 to ptr
  store i64 7, ptr %cast17, align 4
  %field_ptr18 = getelementptr ptr, ptr %record_buffer15, i64 0
  store ptr %field_mem16, ptr %field_ptr18, align 8
  %field_ptr19 = getelementptr ptr, ptr %record_buffer15, i64 1
  store ptr @str.2, ptr %field_ptr19, align 8
  %field_mem20 = call ptr @malloc(i64 8)
  %cast21 = bitcast ptr %field_mem20 to ptr
  store i64 101, ptr %cast21, align 4
  %field_ptr22 = getelementptr ptr, ptr %record_buffer15, i64 2
  store ptr %field_mem20, ptr %field_ptr22, align 8
  %val_to_ptr23 = bitcast ptr %record_buffer15 to ptr
  %elem_ptr24 = getelementptr ptr, ptr %arr_data, i64 2
  store ptr %val_to_ptr23, ptr %elem_ptr24, align 8
  %x_load25 = load ptr, ptr @x, align 8
  %record_buffer26 = call ptr @malloc(i64 24)
  %field_mem27 = call ptr @malloc(i64 8)
  %cast28 = bitcast ptr %field_mem27 to ptr
  store i64 5, ptr %cast28, align 4
  %field_ptr29 = getelementptr ptr, ptr %record_buffer26, i64 0
  store ptr %field_mem27, ptr %field_ptr29, align 8
  %field_ptr30 = getelementptr ptr, ptr %record_buffer26, i64 1
  store ptr @str.3, ptr %field_ptr30, align 8
  %field_mem31 = call ptr @malloc(i64 8)
  %cast32 = bitcast ptr %field_mem31 to ptr
  store i64 80, ptr %cast32, align 4
  %field_ptr33 = getelementptr ptr, ptr %record_buffer26, i64 2
  store ptr %field_mem31, ptr %field_ptr33, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %x_load25, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %val_to_ptr34 = bitcast ptr %record_buffer26 to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr, align 4
  %cap = load i64, ptr %cap_ptr, align 4
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %add_cont

grow:                                             ; preds = %entry
  %3 = icmp eq i64 %cap, 0
  %4 = mul i64 %cap, 2
  %new_cap = select i1 %3, i64 4, i64 %4
  %new_byte_size = mul i64 %new_cap, 8
  %realloc_ptr = call ptr @realloc(ptr %data_ptr, i64 %new_byte_size)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %realloc_ptr, ptr %data_ptr_ptr, align 8
  br label %add_cont

add_cont:                                         ; preds = %grow, %entry
  %final_data_ptr = phi ptr [ %data_ptr, %entry ], [ %realloc_ptr, %grow ]
  %target_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %val_to_ptr34, ptr %target_ptr, align 8
  %next_len = add i64 %len, 1
  store i64 %next_len, ptr %len_ptr, align 4
  %x_load35 = load ptr, ptr @x, align 8
  %record_buffer36 = call ptr @malloc(i64 24)
  %field_mem37 = call ptr @malloc(i64 8)
  %cast38 = bitcast ptr %field_mem37 to ptr
  store i64 6, ptr %cast38, align 4
  %field_ptr39 = getelementptr ptr, ptr %record_buffer36, i64 0
  store ptr %field_mem37, ptr %field_ptr39, align 8
  %field_ptr40 = getelementptr ptr, ptr %record_buffer36, i64 1
  store ptr @str.4, ptr %field_ptr40, align 8
  %field_mem41 = call ptr @malloc(i64 8)
  %cast42 = bitcast ptr %field_mem41 to ptr
  store i64 70, ptr %cast42, align 4
  %field_ptr43 = getelementptr ptr, ptr %record_buffer36, i64 2
  store ptr %field_mem41, ptr %field_ptr43, align 8
  %rows_field44 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %x_load35, i32 0, i32 1
  %rows_array45 = load ptr, ptr %rows_field44, align 8
  %val_to_ptr46 = bitcast ptr %record_buffer36 to ptr
  %len_ptr47 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array45, i32 0, i32 0
  %cap_ptr48 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array45, i32 0, i32 1
  %data_ptr_ptr49 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array45, i32 0, i32 2
  %len50 = load i64, ptr %len_ptr47, align 4
  %cap51 = load i64, ptr %cap_ptr48, align 4
  %data_ptr52 = load ptr, ptr %data_ptr_ptr49, align 8
  %is_full53 = icmp uge i64 %len50, %cap51
  br i1 %is_full53, label %grow54, label %add_cont55

grow54:                                           ; preds = %add_cont
  %5 = icmp eq i64 %cap51, 0
  %6 = mul i64 %cap51, 2
  %new_cap56 = select i1 %5, i64 4, i64 %6
  %new_byte_size57 = mul i64 %new_cap56, 8
  %realloc_ptr58 = call ptr @realloc(ptr %data_ptr52, i64 %new_byte_size57)
  store i64 %new_cap56, ptr %cap_ptr48, align 4
  store ptr %realloc_ptr58, ptr %data_ptr_ptr49, align 8
  br label %add_cont55

add_cont55:                                       ; preds = %grow54, %add_cont
  %final_data_ptr59 = phi ptr [ %data_ptr52, %add_cont ], [ %realloc_ptr58, %grow54 ]
  %target_ptr60 = getelementptr ptr, ptr %final_data_ptr59, i64 %len50
  store ptr %val_to_ptr46, ptr %target_ptr60, align 8
  %next_len61 = add i64 %len50, 1
  store i64 %next_len61, ptr %len_ptr47, align 4
  %x_load62 = load ptr, ptr @x, align 8
  %record_buffer63 = call ptr @malloc(i64 24)
  %field_mem64 = call ptr @malloc(i64 8)
  %cast65 = bitcast ptr %field_mem64 to ptr
  store i64 7, ptr %cast65, align 4
  %field_ptr66 = getelementptr ptr, ptr %record_buffer63, i64 0
  store ptr %field_mem64, ptr %field_ptr66, align 8
  %field_ptr67 = getelementptr ptr, ptr %record_buffer63, i64 1
  store ptr @str.5, ptr %field_ptr67, align 8
  %field_mem68 = call ptr @malloc(i64 8)
  %cast69 = bitcast ptr %field_mem68 to ptr
  store i64 101, ptr %cast69, align 4
  %field_ptr70 = getelementptr ptr, ptr %record_buffer63, i64 2
  store ptr %field_mem68, ptr %field_ptr70, align 8
  %rows_field71 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %x_load62, i32 0, i32 1
  %rows_array72 = load ptr, ptr %rows_field71, align 8
  %val_to_ptr73 = bitcast ptr %record_buffer63 to ptr
  %len_ptr74 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array72, i32 0, i32 0
  %cap_ptr75 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array72, i32 0, i32 1
  %data_ptr_ptr76 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array72, i32 0, i32 2
  %len77 = load i64, ptr %len_ptr74, align 4
  %cap78 = load i64, ptr %cap_ptr75, align 4
  %data_ptr79 = load ptr, ptr %data_ptr_ptr76, align 8
  %is_full80 = icmp uge i64 %len77, %cap78
  br i1 %is_full80, label %grow81, label %add_cont82

grow81:                                           ; preds = %add_cont55
  %7 = icmp eq i64 %cap78, 0
  %8 = mul i64 %cap78, 2
  %new_cap83 = select i1 %7, i64 4, i64 %8
  %new_byte_size84 = mul i64 %new_cap83, 8
  %realloc_ptr85 = call ptr @realloc(ptr %data_ptr79, i64 %new_byte_size84)
  store i64 %new_cap83, ptr %cap_ptr75, align 4
  store ptr %realloc_ptr85, ptr %data_ptr_ptr76, align 8
  br label %add_cont82

add_cont82:                                       ; preds = %grow81, %add_cont55
  %final_data_ptr86 = phi ptr [ %data_ptr79, %add_cont55 ], [ %realloc_ptr85, %grow81 ]
  %target_ptr87 = getelementptr ptr, ptr %final_data_ptr86, i64 %len77
  store ptr %val_to_ptr73, ptr %target_ptr87, align 8
  %next_len88 = add i64 %len77, 1
  store i64 %next_len88, ptr %len_ptr74, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 8
  %data_ptr89 = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %x_load62, ptr %data_ptr89, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
